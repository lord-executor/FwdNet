# Overview
TODO

## Creating a Self-Signed RootCA and Derived Host Certificate

Create Root CA certificate
```powershell
$rootCert = New-SelfSignedCertificate -CertStoreLocation Cert:\CurrentUser\My `
  -Subject "LAR RootCA" `
  -FriendlyName "LAR self-signed RootCA" `
  -TextExtension @("2.5.29.19={text}CA=true") `
  -NotAfter (Get-Date).AddMonths(60) `
  -KeyUsage CertSign,CrlSign,DigitalSignature
```

Create wildcard hostname certificate
```powershell
$cert = New-SelfSignedCertificate -CertStoreLocation "cert:\CurrentUser\My" `
   -DnsName @("*.fwd.local") `
   -FriendlyName "Wildcard certificate for *.fwd.local" `
   -Signer $rootCert
```

Export the certificates as a PKCS #12 .pfx file
```powershell
$password = ConvertTo-SecureString "pwd" -AsPlainText -Force
Export-PfxCertificate -Cert $rootCert -FilePath "./lar-root-ca.pfx" -Password $password
Export-PfxCertificate -Cert $cert -FilePath "./wildcard.fwd.local.pfx" -Password $password
```

Now we can extract the certificates and private keys in PEM format and store them in separate files.
```shell
openssl pkcs12 -in .\lar-root-ca.pfx -nocerts -nodes -out lar-root-ca.pem.key
openssl pkcs12 -in .\lar-root-ca.pfx -clcerts -nokeys -out lar-root-ca.pem.crt

openssl pkcs12 -in .\wildcard.fwd.local.pfx -nocerts -nodes -out wildcard.fwd.local.pfx.pem.key
openssl pkcs12 -in .\wildcard.fwd.local.pfx -clcerts -nokeys -out wildcard.fwd.local.pfx.pem.crt
```

## Creating Self-Signed Certificates

In an **elevated** command prompt, run
```powershell
$cert = New-SelfSignedCertificate -DnsName @("fwd.local") -CertStoreLocation "cert:\LocalMachine\My"
```

Using the Windows certificate manager, you can then export this certificate from the certificate store as a .pfx file
(PKCS #12).