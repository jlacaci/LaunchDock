# ?? URGENTE: ARCHIVO CSPROJ CORRUPTO - ACCIÓN REQUERIDA

## ?? Problema Actual

El archivo `LaunchDock.csproj` se ha corrompido durante los cambios y necesita ser recreado.

## ?? PASOS PARA SOLUCIONAR (IMPORTANTE)

### 1. **CIERRA Visual Studio COMPLETAMENTE**
   - Guarda cualquier cambio que tengas abierto en otros archivos
   - Cierra Visual Studio (no solo el proyecto, ciérralo completamente)

### 2. **Ejecuta este comando en PowerShell**

Abre PowerShell en la carpeta del proyecto y ejecuta:

```powershell
cd "D:\OneDrive\Documents\VisualStudio Proyectos\LaunchDock"

@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <AssemblyName>LaunchDock</AssemblyName>
    <RootNamespace>LaunchDock</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>

    <!-- IMPORTANTE: Deshabilitar trimming (incompatible con Windows Forms) -->
    <PublishTrimmed>false</PublishTrimmed>

    <!-- Información de la aplicación -->
    <ApplicationIcon>LaunchDock.ico</ApplicationIcon>
    <Version>1.0.0</Version>
    <Authors>Tu Nombre</Authors>
    <Company>Tu Compañía</Company>
    <Product>LaunchDock</Product>
    <Description>Barra de lanzamiento rápido personalizable para Windows</Description>
    <Copyright>Copyright © 2024</Copyright>
  </PropertyGroup>

  <!-- Configuraciones solo para publicación -->
  <PropertyGroup Condition="`$(Configuration) == 'Release'">
    <PublishTrimmed>false</PublishTrimmed>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <DebugType>none</DebugType>
    <DebugSymbols>false</DebugSymbols>
    <InvariantGlobalization>false</InvariantGlobalization>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
  </ItemGroup>
  <ItemGroup>
    <Resource Include="Resources\**\*" />
  </ItemGroup>
  <ItemGroup>
    <None Include="LaunchDock.ico">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
"@ | Out-File -FilePath "LaunchDock.csproj" -Encoding UTF8
```

### 3. **Verifica que se creó correctamente**

```powershell
dotnet build -c Debug
```

Deberías ver: `Compilación realizado correctamente`

### 4. **Abre Visual Studio de nuevo**

Ahora puedes abrir Visual Studio y el proyecto debería funcionar correctamente.

---

## ?? Alternativa: Copia Manual

Si el comando de PowerShell no funciona, crea manualmente el archivo `LaunchDock.csproj` con este contenido:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <AssemblyName>LaunchDock</AssemblyName>
    <RootNamespace>LaunchDock</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>

    <!-- IMPORTANTE: Deshabilitar trimming (incompatible con Windows Forms) -->
    <PublishTrimmed>false</PublishTrimmed>

    <!-- Información de la aplicación -->
    <ApplicationIcon>LaunchDock.ico</ApplicationIcon>
    <Version>1.0.0</Version>
    <Authors>Tu Nombre</Authors>
    <Company>Tu Compañía</Company>
    <Product>LaunchDock</Product>
    <Description>Barra de lanzamiento rápido personalizable para Windows</Description>
    <Copyright>Copyright © 2024</Copyright>
  </PropertyGroup>

  <!-- Configuraciones solo para publicación -->
  <PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <PublishTrimmed>false</PublishTrimmed>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <DebugType>none</DebugType>
    <DebugSymbols>false</DebugSymbols>
    <InvariantGlobalization>false</InvariantGlobalization>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
  </ItemGroup>
  <ItemGroup>
    <Resource Include="Resources\**\*" />
  </ItemGroup>
  <ItemGroup>
    <None Include="LaunchDock.ico">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

---

## ?? Resumen

1. ? Cierra Visual Studio
2. ? Ejecuta el comando de PowerShell O crea el archivo manualmente
3. ? Verifica con `dotnet build`
4. ? Abre Visual Studio nuevamente

El proyecto debería funcionar correctamente después de estos pasos.
