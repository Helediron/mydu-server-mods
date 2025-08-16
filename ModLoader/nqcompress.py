#!/usr/bin/env python3
"""
NQ Compress - Python implementation of the NQ compression format
Compatible with nqlibs/nqcompress/source/Compress.cpp

Format:
- 4 bytes: Magic number (0x124f0359 for zlib)
- 8 bytes: Uncompressed size (little-endian uint64)
- N bytes: Compressed data using zlib

Usage:
    python nqcompress.py input_file [output_file]
    
If output_file is not specified, it will be input_file + ".nqz"
"""

import sys
import struct
import zlib
import os
from pathlib import Path

# Magic numbers from Compress.cpp
MAGIC_ZLIB = 0x124f0359
MAGIC_LZ4 = 0xfb14b6f9
MAGIC_UNCOMPRESSED = 0x8c488fe9

def compress_file(input_path, output_path=None, compression_level=9):
    """
    Compress a file using the NQ zlib format.
    
    Args:
        input_path: Path to input file
        output_path: Path to output file (optional)
        compression_level: Compression level 1-9 (9 = best compression)
    
    Returns:
        tuple: (success: bool, message: str)
    """
    try:
        input_file = Path(input_path)
        if not input_file.exists():
            return False, f"Input file not found: {input_path}"
        
        if output_path is None:
            output_file = input_file.with_suffix(input_file.suffix + ".nqz")
        else:
            output_file = Path(output_path)
        
        # Read input file
        with open(input_file, 'rb') as f:
            original_data = f.read()
        
        original_size = len(original_data)
        print(f"Original size: {original_size} bytes")
        
        # Compress data using zlib with specified compression level
        compressed_data = zlib.compress(original_data, compression_level)
        compressed_size = len(compressed_data)
        
        print(f"Compressed size: {compressed_size} bytes")
        print(f"Compression ratio: {compressed_size / original_size * 100:.1f}%")
        
        # Create NQ format header + payload
        with open(output_file, 'wb') as f:
            # Write 4-byte magic number (little-endian uint32)
            f.write(struct.pack('<I', MAGIC_ZLIB))
            
            # Write 8-byte uncompressed size (little-endian uint64) 
            f.write(struct.pack('<Q', original_size))
            
            # Write compressed data
            f.write(compressed_data)
        
        final_size = output_file.stat().st_size
        print(f"Output file: {output_file}")
        print(f"Final size: {final_size} bytes (including 12-byte header)")
        
        return True, f"Successfully compressed {input_file} -> {output_file}"
        
    except Exception as e:
        return False, f"Error compressing file: {e}"

def decompress_file(input_path, output_path=None):
    """
    Decompress an NQ format file.
    
    Args:
        input_path: Path to compressed file
        output_path: Path to output file (optional)
    
    Returns:
        tuple: (success: bool, message: str)
    """
    try:
        input_file = Path(input_path)
        if not input_file.exists():
            return False, f"Input file not found: {input_path}"
        
        if output_path is None:
            # Remove .nqz extension if present
            if input_file.suffix == ".nqz":
                output_file = input_file.with_suffix("")
            else:
                output_file = input_file.with_suffix(".decompressed")
        else:
            output_file = Path(output_path)
        
        with open(input_file, 'rb') as f:
            # Read 4-byte magic
            magic_bytes = f.read(4)
            if len(magic_bytes) != 4:
                return False, "File too short - missing magic number"
            
            magic = struct.unpack('<I', magic_bytes)[0]
            
            # Read 8-byte size
            size_bytes = f.read(8)
            if len(size_bytes) != 8:
                return False, "File too short - missing size header"
            
            original_size = struct.unpack('<Q', size_bytes)[0]
            
            # Read compressed data
            compressed_data = f.read()
        
        print(f"Magic: 0x{magic:08x}")
        print(f"Expected size: {original_size} bytes")
        print(f"Compressed data size: {len(compressed_data)} bytes")
        
        # Check magic number
        if magic == MAGIC_ZLIB:
            print("Format: zlib compression")
            decompressed_data = zlib.decompress(compressed_data)
        elif magic == MAGIC_LZ4:
            return False, "LZ4 decompression not implemented in this script"
        elif magic == MAGIC_UNCOMPRESSED:
            print("Format: uncompressed")
            decompressed_data = compressed_data
        else:
            return False, f"Unknown magic number: 0x{magic:08x}"
        
        # Verify size
        if len(decompressed_data) != original_size:
            return False, f"Size mismatch: got {len(decompressed_data)}, expected {original_size}"
        
        # Write decompressed data
        with open(output_file, 'wb') as f:
            f.write(decompressed_data)
        
        print(f"Output file: {output_file}")
        print(f"Decompressed size: {len(decompressed_data)} bytes")
        
        return True, f"Successfully decompressed {input_file} -> {output_file}"
        
    except Exception as e:
        return False, f"Error decompressing file: {e}"

def print_info(file_path):
    """Print information about an NQ compressed file."""
    try:
        with open(file_path, 'rb') as f:
            magic_bytes = f.read(4)
            if len(magic_bytes) != 4:
                print("Error: File too short")
                return
            
            magic = struct.unpack('<I', magic_bytes)[0]
            
            size_bytes = f.read(8)
            if len(size_bytes) != 8:
                print("Error: File too short")
                return
            
            original_size = struct.unpack('<Q', size_bytes)[0]
            
            # Get file size
            f.seek(0, 2)  # Seek to end
            file_size = f.tell()
            compressed_size = file_size - 12  # Subtract header
        
        print(f"File: {file_path}")
        print(f"Magic: 0x{magic:08x}", end="")
        
        if magic == MAGIC_ZLIB:
            print(" (zlib)")
        elif magic == MAGIC_LZ4:
            print(" (LZ4)")
        elif magic == MAGIC_UNCOMPRESSED:
            print(" (uncompressed)")
        else:
            print(" (unknown)")
        
        print(f"Original size: {original_size} bytes")
        print(f"Compressed size: {compressed_size} bytes")
        print(f"Total file size: {file_size} bytes")
        if original_size > 0:
            print(f"Compression ratio: {compressed_size / original_size * 100:.1f}%")
        
    except Exception as e:
        print(f"Error reading file: {e}")

def main():
    if len(sys.argv) < 2:
        print("Usage:")
        print("  Compress:   python nqcompress.py <input_file> [output_file]")
        print("  Decompress: python nqcompress.py -d <input_file> [output_file]")
        print("  Info:       python nqcompress.py -i <file>")
        print("  Help:       python nqcompress.py -h")
        sys.exit(1)
    
    if sys.argv[1] == "-h" or sys.argv[1] == "--help":
        print(__doc__)
        sys.exit(0)
    
    if sys.argv[1] == "-i" or sys.argv[1] == "--info":
        if len(sys.argv) < 3:
            print("Error: No file specified for info")
            sys.exit(1)
        print_info(sys.argv[2])
        sys.exit(0)
    
    if sys.argv[1] == "-d" or sys.argv[1] == "--decompress":
        if len(sys.argv) < 3:
            print("Error: No input file specified")
            sys.exit(1)
        
        input_file = sys.argv[2]
        output_file = sys.argv[3] if len(sys.argv) > 3 else None
        
        success, message = decompress_file(input_file, output_file)
        print(message)
        sys.exit(0 if success else 1)
    
    # Default: compress
    input_file = sys.argv[1]
    output_file = sys.argv[2] if len(sys.argv) > 2 else None
    
    success, message = compress_file(input_file, output_file)
    print(message)
    sys.exit(0 if success else 1)

if __name__ == "__main__":
    main()