all: build

clean:
	@./make.sh clean

build: clean
	@./make.sh build

release: clean
	@./make.sh release

docker: clean
	@./make.sh docker
