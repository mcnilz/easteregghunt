# PowerShell Script to convert FluentAssertions to NUnit Assertions
$integrationTestFiles = @(
    "tests/EasterEggHunt.Infrastructure.Tests/Integration/AdminUserRepositoryIntegrationTests.cs",
    "tests/EasterEggHunt.Infrastructure.Tests/Integration/CampaignRepositoryIntegrationTests.cs", 
    "tests/EasterEggHunt.Infrastructure.Tests/Integration/FindRepositoryIntegrationTests.cs",
    "tests/EasterEggHunt.Infrastructure.Tests/Integration/QrCodeRepositoryIntegrationTests.cs",
    "tests/EasterEggHunt.Infrastructure.Tests/Integration/SessionRepositoryIntegrationTests.cs",
    "tests/EasterEggHunt.Infrastructure.Tests/Integration/UserRepositoryIntegrationTests.cs"
)

foreach ($file in $integrationTestFiles) {
    if (Test-Path $file) {
        Write-Host "Converting $file..."
        $content = Get-Content $file -Raw
        
        # Common FluentAssertions to NUnit conversions
        $content = $content -replace '\.Should\(\)\.NotBeNull\(\)', '.Should().NotBeNull()' # Keep this for now
        $content = $content -replace '\.Should\(\)\.BeNull\(\)', '.Should().BeNull()' # Keep this for now
        $content = $content -replace '\.Should\(\)\.Be\(([^)]+)\)', '.Should().Be($1)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.NotBe\(([^)]+)\)', '.Should().NotBe($1)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.BeTrue\(\)', '.Should().BeTrue()' # Keep this for now
        $content = $content -replace '\.Should\(\)\.BeFalse\(\)', '.Should().BeFalse()' # Keep this for now
        $content = $content -replace '\.Should\(\)\.BeGreaterThan\(([^)]+)\)', '.Should().BeGreaterThan($1)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.BeLessThan\(([^)]+)\)', '.Should().BeLessThan($1)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.HaveCount\(([^)]+)\)', '.Should().HaveCount($1)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.BeEmpty\(\)', '.Should().BeEmpty()' # Keep this for now
        $content = $content -replace '\.Should\(\)\.NotBeEmpty\(\)', '.Should().NotBeEmpty()' # Keep this for now
        $content = $content -replace '\.Should\(\)\.Contain\(([^)]+)\)', '.Should().Contain($1)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.NotContain\(([^)]+)\)', '.Should().NotContain($1)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.BeCloseTo\(([^,]+),\s*([^)]+)\)', '.Should().BeCloseTo($1, $2)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.BeAfter\(([^)]+)\)', '.Should().BeAfter($1)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.BeBefore\(([^)]+)\)', '.Should().BeBefore($1)' # Keep this for now
        $content = $content -replace '\.Should\(\)\.AllSatisfy\(([^)]+)\)', '.Should().AllSatisfy($1)' # Keep this for now
        
        # Convert to NUnit Assertions
        $content = $content -replace '(\w+)\.Should\(\)\.NotBeNull\(\)', 'Assert.That($1, Is.Not.Null)'
        $content = $content -replace '(\w+)\.Should\(\)\.BeNull\(\)', 'Assert.That($1, Is.Null)'
        $content = $content -replace '(\w+)\.Should\(\)\.Be\(([^)]+)\)', 'Assert.That($1, Is.EqualTo($2))'
        $content = $content -replace '(\w+)\.Should\(\)\.NotBe\(([^)]+)\)', 'Assert.That($1, Is.Not.EqualTo($2))'
        $content = $content -replace '(\w+)\.Should\(\)\.BeTrue\(\)', 'Assert.That($1, Is.True)'
        $content = $content -replace '(\w+)\.Should\(\)\.BeFalse\(\)', 'Assert.That($1, Is.False)'
        $content = $content -replace '(\w+)\.Should\(\)\.BeGreaterThan\(([^)]+)\)', 'Assert.That($1, Is.GreaterThan($2))'
        $content = $content -replace '(\w+)\.Should\(\)\.BeLessThan\(([^)]+)\)', 'Assert.That($1, Is.LessThan($2))'
        $content = $content -replace '(\w+)\.Should\(\)\.HaveCount\(([^)]+)\)', 'Assert.That($1, Has.Count.EqualTo($2))'
        $content = $content -replace '(\w+)\.Should\(\)\.BeEmpty\(\)', 'Assert.That($1, Is.Empty)'
        $content = $content -replace '(\w+)\.Should\(\)\.NotBeEmpty\(\)', 'Assert.That($1, Is.Not.Empty)'
        $content = $content -replace '(\w+)\.Should\(\)\.Contain\(([^)]+)\)', 'Assert.That($1, Has.Some.Matches<object>(x => x == $2))'
        $content = $content -replace '(\w+)\.Should\(\)\.NotContain\(([^)]+)\)', 'Assert.That($1, Has.None.Matches<object>(x => x == $2))'
        $content = $content -replace '(\w+)\.Should\(\)\.BeCloseTo\(([^,]+),\s*([^)]+)\)', 'Assert.That($1, Is.EqualTo($2).Within($3))'
        $content = $content -replace '(\w+)\.Should\(\)\.BeAfter\(([^)]+)\)', 'Assert.That($1, Is.GreaterThan($2))'
        $content = $content -replace '(\w+)\.Should\(\)\.BeBefore\(([^)]+)\)', 'Assert.That($1, Is.LessThan($2))'
        
        Set-Content $file $content -NoNewline
        Write-Host "Converted $file"
    }
}

Write-Host "Conversion complete!"
