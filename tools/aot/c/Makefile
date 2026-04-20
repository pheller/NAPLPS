# tools/aot/c/Makefile
#
# Builds the C example. Expects the AOT-published NAPLPS library at
# ../publish/NAPLPS.dll (Windows) or ../publish/libNAPLPS.so (Linux/Mac).
# Run the publish step first:
#
#     dotnet publish ../../../NAPLPS/NAPLPS.csproj -c Release -r <rid> \
#         --property:PublishAot=true -o ../publish
#
# Then:
#
#     make              # compile the demo
#     make run          # build + run against hello.nap
#     make clean

CC       ?= gcc
CFLAGS   ?= -O2 -Wall -Wextra -std=c11
INCLUDES := -I../include
LIBDIR   := ../publish
LDFLAGS  := -L$(LIBDIR) -lNAPLPS

BIN := naplps_demo

ifeq ($(OS),Windows_NT)
    BIN := $(BIN).exe
    LIB := $(LIBDIR)/NAPLPS.dll
else
    LIB := $(LIBDIR)/libNAPLPS.so
endif

.PHONY: all clean run

all: $(BIN)

$(BIN): main.c ../include/naplps.h
	$(CC) $(CFLAGS) $(INCLUDES) $< $(LDFLAGS) -o $@

run: $(BIN)
	./$(BIN) ../../../Examples/telidraw/hello.nap hello.png

clean:
	rm -f $(BIN) *.png *.o
