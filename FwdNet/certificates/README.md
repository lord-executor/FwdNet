# Overview

These are _obviously_ just sample certificates for development purposes and should never be used in any kind of exposed / public scenario.
The certificates are NOT secure, the passwords / private keys are provided here and they apply to the _pseudo_-TLD domain "local" which is not a real thing.

## fwd.local.pfx

- Self-signed single-domain server certificate for domain `fwd.local`
- Includes private and public key
- PFX password is `fwd`

## wildcard.fwd.local.pfx

- Wildcard domain certificate for domain `*.fwd.local`
- Signed by "LAR RootCA"
- Includes private and public key
- PFX password is `pwd`

## LAR RootCA.crt

- Self-signed root CA used to sign the wildcard certificate
- Includes public key only
