# DevOps script to generate a changelog for use in AppCenter by looking at commits between the most recent two tags
[string[]] $twoMostRecentTags = git tag --sort=-version:refname | Select-Object -First 2;
$changeLog = git log --oneline "HEAD...$($twoMostRecentTags[1])";
$changeLog = $changeLog -replace '`n', "$0D$0A"; # URL-encode newlines as CRLFs.
if ([string]::IsNullOrWhiteSpace($changeLog)) {
    $changeLog = "No commits found.";
}

Write-Host "##vso[task.setvariable variable=changeLog]$($changeLog)";