# tools/aot/cpp/Makefile
#
# Builds the C++ example. Expects the AOT-published NAPLPS library at
# ../publish/ (same layout as the C example). Publish first:
#
#     dotnet publish ../../../NAPLPS/NAPLPS.csproj -c Release -r <rid> \
#         --property:PublishAot=true -o ../publish

CXX      ?= g++
CXXFLAGS ?= -O2 -Wall -Wextra -std=c++17
INCLUDES := -I../include
LIBDIR   := ../publish
LDFLAGS  := -L$(LIBDIR) -lNAPLPS

BIN := naplps_demo

ifeq ($(OS),Windows_NT)
    BIN := $(BIN).exe
endif

.PHONY: all clean run

all: $(BIN)

$(BIN): main.cpp ../include/naplps.h
	$(CXX) $(CXXFLAGS) $(INCLUDES) $< $(LDFLAGS) -o $@

run: $(BIN)
	./$(BIN) ../../../Examples/telidraw/hello.nap hello.png

clean:
	rm -f $(BIN) *.png *.o
