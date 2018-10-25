#
# Builds the files that will be used for the package-wrap step.
#
param (
	[string] $version,
  [string] $sourceDir = "Package",
  [string] $destDir = "Output"
)

$projectName = "ShootTheTraps"

if ($version -eq "") {
  echo "Pass -version to supply a version number."
}

if ($version -ne "") { $version = "-$version" }

echo "Packaging $projectName v$version"
echo "Using source directory $sourceDir"
echo "and destination directory $destDir"

New-Item -ItemType Directory -Force -Path "$destDir"

Copy-Item "$sourceDir\$($projectName)_Desktop$version.zip" -Destination "$destDir\$($projectName)_Windows$version.zip"

"Package wrapping on Windows completed."
