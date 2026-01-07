Simple arm converter on C# (GTK)

# Core development tools
- dotnet (net10.0+)
- gcc
- GTK 3/4

# ARM Toolchains
- arm-none-eabi-gcc
- arm-none-eabi-binutils

# ARM64 Toolchains (optional, for ARM64 support)
- aarch64-linux-gnu-gcc
- aarch64-linux-gnu-binutils

# Arch:
```
sudo pacman -S \
  dotnet-sdk \
  gtk-sharp-3 \
  arm-none-eabi-gcc \
  arm-none-eabi-binutils \
  aarch64-linux-gnu-gcc \
  aarch64-linux-gnu-binutils
```

# Ubuntu:
```
sudo apt-get install \
  dotnet-sdk-10.0 \
  libgtk2.0-dev \
  arm-none-eabi-gcc \
  arm-none-eabi-binutils \
  aarch64-linux-gnu-gcc \
  binutils-aarch64-linux-gnu
```

# Fedora:
```
sudo dnf install \
  dotnet-sdk-10.0 \
  gtk-sharp \
  arm-none-eabi-gcc \
  arm-none-eabi-binutils \
  aarch64-linux-gnu-gcc \
  aarch64-linux-gnu-binutils
```

# Build
```
dotnet publish -c Release -o ./publish --self-contained -r linux-x64
```

# Launch
```
arm-converter
```