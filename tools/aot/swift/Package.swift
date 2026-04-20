// swift-tools-version: 5.9
// Package.swift — SwiftPM manifest for the NAPLPS demo.

import PackageDescription

let package = Package(
    name: "naplps-demo",
    targets: [
        // System library target exposes the C header + library to Swift via a
        // module map. The module map declares `module CNaplps { header "../include/naplps.h" link "NAPLPS" }`.
        .systemLibrary(
            name: "CNaplps",
            path: "Sources/CNaplps",
            pkgConfig: nil
        ),
        .executableTarget(
            name: "naplps_demo",
            dependencies: ["CNaplps"],
            path: "Sources/naplps_demo",
            linkerSettings: [
                .unsafeFlags(["-L", "../publish"]),
                .linkedLibrary("NAPLPS"),
            ]
        ),
    ]
)
