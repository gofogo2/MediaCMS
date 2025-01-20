# 관리자 권한으로 실행해야 합니다.
# 변수 설정
$Username = "ftpuser"
$Password = "1"
$FtpSiteName = "contentsServer"
$FtpRoot = "C:\contents"

# Windows 기능 설치 함수 (DISM 사용)
function Install-WindowsFeature {
    $FeatureList = @(
        "IIS-FTPServer",
        "IIS-FTPSvc"
    )
    
    foreach ($Feature in $FeatureList) {
        Write-Host "Installing $Feature..."
        $Result = dism /online /enable-feature /featurename:$Feature /all /norestart
    }
    return $true
}

# 사용자 계정 생성 함수
function Create-LocalUser {
    param([string]$Username, [string]$Password)
    
    $securePassword = ConvertTo-SecureString $Password -AsPlainText -Force
    New-LocalUser -Name $Username -Password $securePassword -FullName $Username -Description "FTP User" -UserMayNotChangePassword
    Add-LocalGroupMember -Group "Users" -Member $Username
}

# FTP 디렉토리 생성 및 권한 설정 함수
function Create-FtpDirectory {
    param([string]$Path, [string]$Username)
    
    if (-not (Test-Path $Path)) {
        New-Item -Path $Path -ItemType Directory
    }
    $Acl = Get-Acl $Path
    $Ar = New-Object System.Security.AccessControl.FileSystemAccessRule($Username, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $Acl.SetAccessRule($Ar)
    Set-Acl $Path $Acl
}

# FTP 사이트 생성 함수
function Create-FtpSite {
    param([string]$SiteName, [string]$PhysicalPath)
    
    Import-Module WebAdministration
    if (-not (Get-Website -Name $SiteName)) {
        New-WebFtpSite -Name $SiteName -PhysicalPath $PhysicalPath -Force
    }
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name ftpServer.security.ssl.controlChannelPolicy -Value 0
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name ftpServer.security.ssl.dataChannelPolicy -Value 0

    # FTP 인증 설정
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name ftpServer.security.authentication.basicAuthentication.enabled -Value $true
    
    # 사이트 바인딩 설정
    New-WebBinding -Name $SiteName -Protocol ftp -IPAddress "*" -Port 21
}

# FTP 사용자 권한 설정 함수
function Set-FtpUserPermission {
    param([string]$SiteName, [string]$Username)
    
    $ConfigPath = "IIS:\Sites\$SiteName"
    
    # FTP 권한 설정 (부모 수준에서 잠금 해제)
    Set-WebConfigurationProperty -PSPath $ConfigPath -Filter "/system.ftpServer/security/authorization" -Name "overrideMode" -Value "Allow"
    
    Add-WebConfiguration -Filter "/system.ftpServer/security/authorization" -PSPath $ConfigPath -Value @{
        accessType="Allow"
        users=$Username
        permissions="Read,Write"
    }
}

# 메인 실행 부분
try {
    # FTP 서버 설치
    $Success = Install-WindowsFeature
    if (-not $Success) {
        throw "FTP 서버 설치에 실패했습니다."
    }
    
    # 사용자 계정 생성
    Create-LocalUser -Username $Username -Password $Password
    
    Write-Host "완료되었습니다"
} catch {
    Write-Host "오류 발생: $_"
}


