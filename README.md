# Reference Architecture for Web Applications

## Project goal

An example solution that uses best practice patterns for developing web applications.


### Frontend Patterns

<label hidden="hidden">* Structure of the web: HTML</label>
<label hidden="hidden">* Styles of the Web: CSS</label>
<label hidden="hidden">* Language of the Web: Javascript</label>

### API Patterns

* [Vertical Slice Architecture](/Studies/1.vertical_slice_architecture.md)

<label hidden="hidden">* Endpoint Load Balancing(calculate endpoint weight and create or delete nodes, and balance endpoints on nodes)</label>
<label hidden="hidden">* Event Sourcing</label>
<label hidden="hidden">* Caching</label>
<label hidden="hidden">* Rete-Limiting</label>
<label hidden="hidden">* Partition gated clearance(sepparate data in partitions, isolate and update individual partitions, and enable
updated nodes to work with the newly updated partitions of data)</label>
<label hidden="hidden">* Types of connections: HTTP, REST, gRPS, WebSockets, SignalR, MQTT, TCP, UDP</label>
<label hidden="hidden">* Types of connections: HTTP 1 vs 2 vs 3</label>

### Application Testing Patterns
<label hidden="hidden">* In Memory Testing Of Applications</label>
<label hidden="hidden">* Performance testing</label>
<label hidden="hidden">* Penetration testing</label>

### Infrastructure Patterns

* [Cryptography for the web : OpenSSL](/Studies/3.cryptography_for_web.md)
* [Document Databases](/Studies/2.document_databases.md)
* [Public Key Infrastructure : OpenSSL](/Studies/4.public_key_infrastructure.md)
* [Reverse Proxy](/Studies/5.reverse_proxy.md)

<label hidden="hidden">* Reverse proxy real-time configuration</label>
<label hidden="hidden">* Serverless</label>