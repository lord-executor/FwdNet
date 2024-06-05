# Overview
TODO

## Creating Self-Signed Certificates

In an **elevated** command prompt, run
```powershell
$cert = New-SelfSignedCertificate -DnsName @("fwd.local") -CertStoreLocation "cert:\LocalMachine\My"
```

Using the Windows certificate manager, you can then export this certificate from the certificate store as a .pfx file
(PKCS #12).