#!/usr/bin/env ruby
# tools/aot/ruby/naplps_demo.rb
#
# Load the NAPLPS NativeAOT library via the `ffi` gem, render a .nap file to
# PNG.
#
# Install the gem once:
#     gem install ffi
#
# Usage:
#     ruby naplps_demo.rb <input.nap> <output.png> [width] [height]

require 'ffi'

module Naplps
  extend FFI::Library

  def self.library_path
    dir = File.expand_path('../publish', __dir__)
    case RUBY_PLATFORM
    when /mswin|mingw|cygwin|ucrt/ then File.join(dir, 'NAPLPS.dll')
    when /darwin/                  then File.join(dir, 'libNAPLPS.dylib')
    else                                File.join(dir, 'libNAPLPS.so')
    end
  end

  ffi_lib library_path

  # Signatures match tools/aot/include/naplps.h.
  attach_function :naplps_version,       [:pointer, :int32], :int32
  attach_function :naplps_command_count, [:pointer, :int32], :int32
  attach_function :naplps_error_count,   [:pointer, :int32], :int32
  attach_function :naplps_render_png,    [:pointer, :int32, :int32, :int32, :pointer, :int32], :int32
end

def main(argv)
  if argv.length < 2
    warn "usage: #{$PROGRAM_NAME} <input.nap> <output.png> [width] [height]"
    return 2
  end

  in_path  = argv[0]
  out_path = argv[1]
  width    = (argv[2] || 1024).to_i
  height   = (argv[3] || 768).to_i

  version_buf = FFI::MemoryPointer.new(:uint8, 32)
  vlen = Naplps.naplps_version(version_buf, 32)
  if vlen < 0
    warn "naplps_version failed: #{vlen}"
    return 1
  end
  puts "NAPLPS library version: #{version_buf.read_string(vlen)}"

  nap_bytes = File.binread(in_path)
  nap_len = nap_bytes.bytesize
  puts "Loaded #{in_path} (#{nap_len} bytes)"

  nap_ptr = FFI::MemoryPointer.new(:uint8, nap_len)
  nap_ptr.write_bytes(nap_bytes)

  n_cmds = Naplps.naplps_command_count(nap_ptr, nap_len)
  n_errs = Naplps.naplps_error_count(nap_ptr, nap_len)
  puts "Parsed #{n_cmds} commands, #{n_errs} errors"
  return 1 if n_cmds < 0

  # Query required PNG size, then render.
  required = Naplps.naplps_render_png(nap_ptr, nap_len, width, height, nil, 0)
  if required < 0
    warn "render failed (query): #{required}"
    return 1
  end

  png_ptr = FFI::MemoryPointer.new(:uint8, required)
  written = Naplps.naplps_render_png(nap_ptr, nap_len, width, height, png_ptr, required)
  if written < 0
    warn "render failed: #{written}"
    return 1
  end

  File.binwrite(out_path, png_ptr.read_bytes(written))
  puts "Wrote #{out_path} (#{written} bytes, #{width}x#{height})"
  0
end

exit(main(ARGV))
