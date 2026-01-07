Simple arm converter on C# (GTK)

# Screenshots

<img width="726" height="562" alt="image" src="https://github.com/user-attachments/assets/9db9641a-cd68-4a41-82f8-e29e7babea2a" />

<img width="726" height="562" alt="image" src="https://github.com/user-attachments/assets/4b1fefe0-b8d2-4533-96a7-0f1f9b349d0a" />

<img width="726" height="562" alt="image" src="https://github.com/user-attachments/assets/abb510c0-e385-4e1c-a628-820b38d4649a" />

<img width="726" height="562" alt="image" src="https://github.com/user-attachments/assets/db623a1b-e929-41fd-a307-50011d90ae06" />


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
