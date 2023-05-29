# Configuring HTTPS for an ASP.Net application

## Commands to generate a Self-Signed SSL/TLS certificate

All following commands are for powershell.

```
// Set certificate common name

$certname = "localhost"    

// Create self signed in Personal Store (Server authentication only and Time stamping)

$cert = New-SelfSignedCertificate -Subject "CN=$certname" -DnsName "$certname" -CertStoreLocation "cert:\LocalMachine\My" -NotAfter (Get-Date).AddYears(20) -FriendlyName "ASP.NET Core HTTPS development certificate" -KeyExportPolicy Exportable -KeySpec Signature -KeyUsage DigitalSignature, KeyEncipherment -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256 -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1,1.3.6.1.5.5.7.3.8")

// Move to Trusted Root Certificate Authorities

Move-Item (Join-Path Cert:\LocalMachine\My $cert.Thumbprint) -Destination Cert:\LocalMachine\Root

// Create encryption password

$certpass = ConvertTo-SecureString -String "MySecureStrongPassword" -Force -AsPlainText

// Export certificate with key

Export-PfxCertificate -Cert $cert -FilePath "C:\certs\$certname.pfx" -Password $certpass

// Export certificate without key

Export-Certificate -Cert $cert -FilePath "C:\certs\$certname-nokey.cer" 
```

## Kestrel manual appsettings configuration

```
"Kestrel": {
    "EndPoints": {
      "Https": {
        "Url": "https://localhost:5149",
        "Certificate": {
          "Path": "C:\\certs\\localhost.pfx",
          "Password": "MySecureStrongPassword"
        }
      }
    }
  },
```

or
```
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "C:\\certs\\localhost.pfx",
        "Password": "MySecureStrongPassword"
      }
    }
  },
```

## Locations of certificates (LocalMachine, CurrentUser)		

To view certificates in the current logged in user store, open Run:
```
certmgr. msc
```
To view certificates in the current machine store, open Run:
```
certlm.msc
```

## Internal configuration for auto development certificate resolution
```
internal const string AspNetHttpsOid = "1.3.6.1.4.1.311.84.1.1";
internal const string AspNetHttpsOidFriendlyName = "ASP.NET Core HTTPS development certificate";

private const string ServerAuthenticationEnhancedKeyUsageOid = "1.3.6.1.5.5.7.3.1";
private const string ServerAuthenticationEnhancedKeyUsageOidFriendlyName = "Server Authentication";
```		
## OpenSSL commands to examine the generated certificate

```
// Extract private key

openssl pkcs12 -in localhost-cert.pfx -nocerts -out localhost.key

// Extract certificate from PfxCertificate

openssl pkcs12 -in localhost-cert.pfx -clcerts -nokeys -out localhost.crt

// Examine certificate

openssl x509 -in localhost.crt -noout -text

// Decrypt private key

openssl rsa -in localhost.key -out localhost.key.dec
```

## Using OpenSSL to create a ASP.Net development certificate(auto resolve)

### Create a Asp.Net configuration file 'asp_net.cnf':
```
[ req ]
default_bits = 2048
distinguished_name = dn
req_extensions = aspnet

[ dn ]
CN = localhost

[ aspnet ]
basicConstraints = critical, CA:FALSE
keyUsage = critical, digitalSignature, keyEncipherment
extendedKeyUsage = critical, serverAuth
subjectAltName = critical, DNS:localhost
1.3.6.1.4.1.311.84.1.1 = DER:02
```

```
// Create CA
TODO

// Create self signed certificate
openssl req -new -config asp_net.cnf -keyout local_asp_dev.key -out local_asp_dev.csr -nodes

// Sign with CA
openssl x509 -req -in local_asp_dev.csr -CA /path/to/CA.pem -CAkey /path/to/CA.key -CAcreateserial -out local_asp_dev.crt -days 365 -sha256 -extensions aspnet -extfile asp_config.conf

// Pack into a pfx file
openssl pkcs12 -in local_asp_dev.crt -inkey local_asp_dev.key -export -out local_asp_dev.pfx
```

## Create certificate using `dev-certs` tool

```
dotnet dev-certs https --trust
```

## Documentation
[Configure HTTPS for ASP.Net](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-7.0)
[New-SelfSignedCertificate Command](https://learn.microsoft.com/en-us/powershell/module/pki/new-selfsignedcertificate?view=windowsserver2022-ps)

[dev-certs](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-dev-certs)