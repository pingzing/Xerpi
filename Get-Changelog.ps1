# DevOps script to generate a changelog for use in AppCenter by looking at commits between the most recent two tags
[string[]] $lastTag = git tag --sort=-version:refname | Select-Object -First 1;
Write-Host "Found the following tags: $($lastTag)";
$changeLog = git log --oneline "HEAD...$($lastTag[0])";
$changeLog = $changeLog -replace '`n', "$0D$0A"; # URL-encode newlines as CRLFs.
Write-Host "Saving the following changelog: $($changeLog)";
Write-Host "##vso[task.setvariable variable=changeLog]$($changeLog)";