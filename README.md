# A set of simulator tools and fixtures

This is a set of simulator tools and fixtures intended for support of the AASeq
project.


## Tools

### TCP Echo

Responds to any TCP traffic by echoing the same traffic back.

    bin/aaseq-echo-tcp -p 1234

### TCP Diameter Echo

Responds to any Diameter traffic by replying with the exactly same AVPs.

    bin/aaseq-echo-tcp-diameter

### UDP Echo

Responds to any UDP traffic by echoing the same traffic back.

    bin/aaseq-echo-udp -p 1234
