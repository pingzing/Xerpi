# DevOps script to generate a changelog for use in AppCenter by looking at commits between the most recent two tags
[string[]] $lastTwoTags = git tag --sort=-version:refname | Select-Object -First 2;
$commitLogs = git log --oneline "$($lastTwoTags[0])..$($lastTwoTags[1])";
Write-Host "##vso[task.setvariable variable=commitLogs]$($commitLogs)";