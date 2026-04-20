// build.rs — tell rustc where to find the NAPLPS native library.
//
// Expects the AOT publish output at tools/aot/publish/, produced by:
//     dotnet publish ../../../NAPLPS/NAPLPS.csproj -c Release -r <rid> \
//         --property:PublishAot=true -o ../publish

fn main()
{
    let manifest_dir = std::env::var("CARGO_MANIFEST_DIR").unwrap();
    let lib_dir = std::path::Path::new(&manifest_dir)
        .parent()
        .unwrap()
        .join("publish");

    println!("cargo:rustc-link-search=native={}", lib_dir.display());
    println!("cargo:rustc-link-lib=dylib=NAPLPS");

    // On Unix the rpath lets the binary find libNAPLPS.so/.dylib at runtime without
    // requiring LD_LIBRARY_PATH. On Windows the DLL needs to sit alongside the .exe
    // (copy step in the Makefile / user workflow).
    #[cfg(not(target_os = "windows"))]
    println!("cargo:rustc-link-arg=-Wl,-rpath,{}", lib_dir.display());
}
