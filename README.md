# Reference Architecture for Web Applications

## Project goal

An example solution that uses best practice patterns for developing web applications.


### Frontend topics

* Structure of the web: HTML
* Styles of the Web: CSS
* Language of the Web: Javascript

### Backend API topics

* [Vertical Slice Architecture](/Studies/1.vertical_slice_architecture.md)
* [Object Relational Mapper : Entity Framework](/Studies/6.object_relational_mappers.md)

* Endpoint Load Balancing(calculate endpoint weight and create or delete nodes, and balance endpoints on nodes)
* Event Sourcing
    * All events have a conversation ID 
    * Partitioned by the conversation type
    * Background service processes queue to save the latest state of a materialized view 
    * Reads only return the materialized view
    * Possibility to view Entire conversations
    * Replace log messages with events and link to the same conversation
* Caching
* Rete-Limiting
* Partition gated clearance(sepparate data in partitions, isolate and update individual partitions, and enable
updated nodes to work with the newly updated partitions of data)
* Types of connections: HTTP, REST, gRPS, WebSockets, SignalR, MQTT, TCP, UDP
* Types of connections: HTTP 1 vs 2 vs 3
* Telemetry
    * Dependency calls ( http, sql, grpc)
    * Costs
    * ITelemetryInitializer, ITelemetryProcessor
    * Distributed tracing/ Conversations
    * System.Diagnostics.Activity/ ActivityId WC3
    * OpenTelemetry
* Behaviour Driven Development
* Functional programming

### Application Testing topics

* In Memory Testing Of Applications
* Performance testing
* Penetration testing
* Test driven development
* Time Studies(Delay,StopWatch,PeriodicTimer)

### Infrastructure topics

* [Cryptography for the web : OpenSSL](/Studies/3.cryptography_for_web.md)
* [Document Databases : CouchDB, MongoDB](/Studies/2.document_databases.md)
* [Public Key Infrastructure : OpenSSL](/Studies/4.public_key_infrastructure.md)
* [Reverse Proxy : YARP](/Studies/5.reverse_proxy.md)

* Application Configuration
* Key Vaults
* Reverse proxy real-time configuration
* Serverless