# FWShellMAUI
```
  <ItemGroup>
    <None Remove="FWITD\ClientApp\**\*" />
    <None Remove="FWITD\out\**" />
    <Content Include="FWITD\ClientApp\**\*.min.js" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="FWITD\ClientApp\**\*.min.css" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="FWITD\ClientApp\**\*.min.html" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="FWITD\ClientApp\**\*.woff2" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="FWITD\ClientApp\**\*.png" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="FWITD\ClientApp\**\*.svg" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="FWITD\**\*.ico" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
    <Content Include="FWITD\Assets\DBUpdate\*.sql" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
```