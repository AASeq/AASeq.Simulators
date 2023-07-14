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


## Other Tools

### radclient

To install, run command below.

    sudo apt install freeradius-utils

To simulate radius packet check [man page](https://man.archlinux.org/man/radclient.1.en)

    echo "User-Name=test,User-Password=changeme" | radclient 127.0.0.1 auth secret

### nc

To simulate a single diameter packet, use the following line:

    echo -ne '\x01\x00\x00\x14\x80\x00\x00\x01\x00\x00\x00\x02\x00\x00\x00\x04\x00\x00\x00\x04' | nc localhost 3868
