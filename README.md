# Overview

`FwdNet` is a very light wrapper around the popular [YARP](https://microsoft.github.io/reverse-proxy/) HTTP proxy server library running on top of ASP.NET Core. It is a tool for developers to set up convenient local domain aliases and use those to access their development services in a way that is as close to a production setup as possible. It is mostly geared towards _Windows_ users, but the core application runs just fine when compiled on Linux or Mac.

Imagine having 3 different web services running locally that together make up a complete application backend. Let's call those services Alpha, Beta and Gamma. On your production system, those services are all accessible through their own subdomains `alpha.api.example.com`, `beta.api.example.com` and `gamma.api.example.com` and you would like to be able to use those same (or slightly different) host names to access your local services which may be running directly on your machine, inside of a Kubernetes cluster that you are running, on a separate machine or any combination of those scenarios.

The first problem that you have to solve is to get valid TLS server certificates so that you can access your services through HTTPS, because without that you would have to deal with a whole bunch of annoying issues. The instructions for how to do that using PowerShell are below in the section about creating self-signed root certificate authorities.

Next, we'll make use of the fact that _loopback addresses_ encompass a whole 24 bit range from the well-known 127.0.0.1 to the rarely used 127.255.255.254 to map our desired host names to a set of such loopback addresses. In the `C:\Windows\System32\drivers\etc\hosts`, we can add the following lines, picking the _arbitrary_ starting point `127.0.12.1`:

```
127.0.12.1 alpha.api.example.com
127.0.12.2 beta.api.example.com
127.0.12.3 gamma.api.example.com
```

If your services are running inside of a local Kubernetes cluster, then you would of course just make sure that you can access the pods directly via their static IPs and use those in the hosts file configuration.

Lastly, we need a `FwdNet` configuration file that we can use for starting the proxy. Assuming that the services are running locally on ports 4567 through 4569 and you have a custom wildcard certificate for `*.api.example.com` installed for your current user, the configuration would look like this:

```json
[
  {
    "Listen": "https://127.0.12.1",
    "ListenHost": "alpha.api.example.com",
    "Forward": "https://localhost:4567/",
    "Certificate": "cert:/CurrentUser/My/CN=*.api.example.com"
  },
  {
    "Listen": "https://127.0.12.2",
    "ListenHost": "beta.api.example.com",
    "Forward": "https://localhost:4568/",
    "Certificate": "cert:/CurrentUser/My/CN=*.api.example.com"
  },
  {
    "Listen": "https://127.0.12.3",
    "ListenHost": "gamma.api.example.com",
    "Forward": "https://localhost:4569/",
    "Certificate": "cert:/CurrentUser/My/CN=*.api.example.com"
  }
]
```

By using different configuration files, you can easily point the same set of domain names to various different locations depending on your needs. For example, the Alpha service could be running on a development environment with the Beta service running as a local Kubernetes pod while the Gamma service is running directly on your host since that is the one currently being worked on.

## Build and Run

Since this is by design a developer tool, the assumption is that you have all the tools you need to build and run `FwdNet` already installed. Those tools are:
- `git` of course
- a .NET SDK >= 8.0
- PowerShell Core >= 7.0.0

1. Clone the repository with `git clone https://github.com/lord-executor/FwdNet.git`
2. Change to that new directory `cd FwdNet`
3. Open your PowerShell profile with your favorite text editor, e.g. with Visual Studio Code `code $PROFILE`
4. Add the following line to the profile `. "C:\full\path\to\FwdNet\fwd.ps1"`
5. Reload your profile with `. $PROFILE`
6. Now you can start using the convenient `fwd` alias that automatically builds and runs the binary

## Creating a Self-Signed RootCA and Derived Host Certificate

Valid certificates are essential for reproducing a production-like usage pattern, but custom certificates are not exactly easy to set up - unless you have a decent guide to follow.

Please note however that the instructions here are for **deveopment** use only and you should only use your own custom certificates for production use if you _really_ know what you are doing - if you are reading this guide, then that is not you.

In this guide, we will set up our own _root certificate authority_ certificate that we can install as a trusted root certificate so that we can then derive multiple TLS server certificates without having to install each of them separately as trusted certificates. The derived certificates are validated by the client (browser or HTTP APIs) by checking the certificate's chain of trust which means that as long as the root certificate is trusted, the certificate is considered valid. Having your own root CA certificate is especially useful if you want to be able to access your development machine from other clients like for example mobile devices running iOS or Android because you only have to install the root CA as trusted and from then on, the devices will accept all derived server certificates.

### Create a Root CA Certificate

Creating a self-signed Root CA certificate actually only requires one command.

```powershell
$rootCert = New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\My `
  -Subject "My Root CA" `
  -FriendlyName "My self-signed RootCA" `
  -TextExtension @("2.5.29.19={text}CA=true") `
  -NotAfter (Get-Date).AddMonths(60) `
  -KeyUsage CertSign,CrlSign,DigitalSignature
```

- The `Subject` and `FriendlyName` are completely up to you, but you should consider the subject name carefully since you will not be able to change it later on.
- `TextExtension` with the cryptic identifier and syntax is what marks this certificate as a CA certificate.
- Since this is a _low security_ development certificate, we can set the expiration to 5 years in the future so that we don't have to worry about it every year.
- With the `KeyUsage` we declare that we are only going to allow this certificate to be used to create _other_, derived certificates.

Now, in most cases, you want to export this certificate in some form so that you can install it on other machines as well. Check the "Exporting Certificates" section below for how to do that.

### Create a Wildcard TLS Server Certificate

With the Root CA from the previous step, we can now create a wildcard hostname certificate for our domain.

```powershell
$cert = New-SelfSignedCertificate -CertStoreLocation "cert:\CurrentUser\My" `
   -DnsName @("*.api.example.com") `
   -FriendlyName "Wildcard certificate for *.api.example.com" `
   -NotAfter (Get-Date).AddMonths(13) `
   -Signer $rootCert
```

### Exporting Certificates

When working with certificates, there are a whole bunch of different formats and names that can get quite confusing. We are sticking to the basics of
- PFX which is most commonly used on Windows and contains both the public and private key in a wrapper structure that is password protected
- Separate ".key" and ".crt" files which represent the private and public key and are commonly used on Linux systems
- ".cer" files which only ever contain the public key of a certificate and is commonly used on Windows, Linux and Mac for installing a certificate as trusted

First, we export the certificates as a PKCS #12 .pfx file because that is the easiest way to get a certificate out of the Windows certificate store into a file. Note that PFX files _MUST_ have a password even though that can sometimes be annoying. We use a simple dummy password here since again, these certificates are only used for local development. For long term storage and distributing the certificates to other users you should use a _password manager_ to store the certificates which will make the PFX password even more pointless.

```powershell
$cert = Get-Item "Cert:\CurrentUser\My\[Thumbprint of the Certificate to Export]"
$password = ConvertTo-SecureString "pwd" -AsPlainText -Force
Export-PfxCertificate -Cert $cert -FilePath "./my-certificate.pfx" -Password $password
Export-Certificate -Cert $cert -Type CERT -FilePath 'my-certificate.crt'
```

Now we can extract the certificates and private keys in PEM format and store them in separate files using the `openssl` tools.
```shell
openssl pkcs12 -in ./my-certificate.pfx -nocerts -nodes -out my-certificate.pem.key
openssl pkcs12 -in ./my-certificate.pfx -clcerts -nokeys -out my-certificate.pem.crt
```

### Install the Root CA as Trusted

Just because we _created_ a new Root CA certificate does not mean that you computer actually trusts that certificate. If we want to install the certificate as a trusted certificate, we ideally want to import it into "Trusted Root Certification Authorities" for your current user (`Cert:\CurrentUser\Root`) or the local machine (`Cert:\LocalMachine\Root`). Certain applications, like code signing require the certificate to be in the local machine store, but TLS certificates only need to be trusted by the user that is validating the certificate chain and this _CurrentUser_ should be preferred.

```powershell
Import-Certificate -FilePath my-certificate.crt -CertStoreLocation "Cert:\CurrentUser\Root"
```

## FwdNet Configuration Format

The configuration format for `FwdNet` is kept deliberately simple. YARP supports many, many more features but this tool only exposes a small subset for a specific use case.

The configuration file is using JSON and at the top level is just an _array_ of [ForwardingRules](https://github.com/lord-executor/FwdNet/blob/main/FwdNet/ForwardingRule.cs) which set up a listener and target each. They have the form

```jsonc
{
    // Scheme and host name / IP on which the proxy will listen for incoming traffic
    "Listen": "https://127.0.12.1",
    // Host name the proxy expects to be used
    "ListenHost": "alpha.api.example.com",
    // Destination host where the HTTP requests will be proxied to
    "Forward": "https://localhost:4567/",
    // Certificate expression, see below
    "Certificate": "C:\\Dev\\my-certificate.pfx",
    // This is only needed if the Certificate points to a PFX file
    "CertificatePassword": "foobar"
}
```

### Certificate Expressions

There are currently two forms of certificate expressions supported:
1. Local path to a PFX file - this option also requires the `CertificatePassword` to be set and the user running `FwdNet` must be able to access the file.
2. PowerShell _style_ certificate store references - a simple URI scheme of the form `cert:/[Store Location]/[Store Name]/[Thumbprint or Common Name Filter]`. Examples
    - Referencing the certificate with common name "*.api.example.com": `cert:/CurrentUser/My/CN=*.api.example.com`
    - Referencing the certificate by thumbprint: `cert:/CurrentUser/My/DF523A2B6BCD8460150E7FBF9B138E9ED9853BB8`
