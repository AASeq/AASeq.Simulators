#!/bin/sh

SCRIPT_PATH="$(realpath "$0")"
SCRIPT_DIR="$(dirname "$SCRIPT_PATH")"

if [ "$#" -gt 1 ]; then
    >&2 echo "Usage: $(basename $SCRIPT_PATH) [clean|build|release]"
    exit 1
fi

ACTION=$1
if [ "$ACTION" = "" ]; then ACTION="all"; fi
if [ "$ACTION" = "all" ]; then ACTION="build"; fi


clean() {
    mkdir -p "$SCRIPT_DIR/bin"
    find "$SCRIPT_DIR/bin" -mindepth 1 -delete
}

build() {
    mkdir -p "$SCRIPT_DIR/bin"
    find "$SCRIPT_DIR/src" -name "*.csproj" -print0 \
        | xargs -0 -I{} \
        dotnet build {} --configuration Debug --output "$SCRIPT_DIR/bin"
}

release() {
    mkdir -p "$SCRIPT_DIR/bin"
    find "$SCRIPT_DIR/src" -name "*.csproj" -print0 \
        | xargs -0 -I{} \
        dotnet publish {} --configuration Release --output "$SCRIPT_DIR/bin" \
            --self-contained true --use-current-runtime \
            -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true
}

docker() {
    mkdir -p "$SCRIPT_DIR/build/docker"
    dotnet publish --configuration Release --output "$SCRIPT_DIR/build/docker" \
        /t:PublishContainer -p:PublishProfile=DefaultContainer --no-self-contained \
        "$SCRIPT_DIR/src/AASeq.Echo.Tcp.Diameter/AASeq.Echo.Tcp.Diameter.csproj"
    dotnet publish --configuration Release --output "$SCRIPT_DIR/build/docker" \
        /t:PublishContainer -p:PublishProfile=DefaultContainer --no-self-contained \
        "$SCRIPT_DIR/src/AASeq.Echo.Udp.Radius/AASeq.Echo.Udp.Radius.csproj"
}


case $ACTION in
    clean)
        clean
        ;;

    build)
        if ! [ -n "$MAKELEVEL" ]; then clean; fi
        build
        ;;

    release)
        if ! [ -n "$MAKELEVEL" ]; then clean; fi
        release
        ;;

    docker)
        if ! [ -n "$MAKELEVEL" ]; then clean; fi
        docker
        ;;

    *)
        >&2 echo "Unknown action $ACTION"
        exit 1
        ;;
esac
