# One-time tool used in Phase 2.7. Adds a permission rule to each Vue route's meta, keeping encoding
# and line endings. Any-of menu lists match the API rules in API/Vsky.Api/Authorization/MenuLinks.cs.
param([string]$ModulesPath)

$work = @('/project','/project-tasks','/my-task-and-activities','/all-project-planner','/project-targetplan/weeklyplanner','/project-targetplan/monthlyplanner','/billing-processing-employee','/project/assign-users-to-project','/project-questions-answers/list','/project-release-tracking/list','/timesheet','/my-daily-planner')

# Pages with no menu row can only require membership of the site.
$siteMember = @('account','dashboard','helpdesk','items','leave-yearly-schedule','movementRegister','notification','settings','sites-items','sites-sharing','timeInTimeOut')
$public = @('auth','website')

$menus = @{
  'candidate'=@('/candidate'); 'company-contacts'=@('/company-contacts','/company'); 'company'=@('/company','/company-contacts','/customer'); 'customer-files'=@('/customer-files','/customer'); 'customer'=@('/customer');
  'department'=@('/department'); 'dropdown'=@('/dropdown','/dropdowntypes'); 'employee-org-structure'=@('/employee-org-structure'); 'employee'=@('/employee'); 'finance-bank-account'=@('/finance-bank-account');
  'finance-expense-advance'=@('/advance-expense-request','/approve-advance-expense'); 'finance-expense-purchase-request'=@('/finance-expense-purchase-request','/approve-purchase-expense'); 'finance-expense-vendors'=@('/finance-expense-vendors'); 'finance-expense'=@('/finance-expense','/approve-expense');
  'infra-account-services'=@('/infra-account-services'); 'infra-account'=@('/infra-account'); 'infra-dashboard'=@('/infra-account','/infra-account-services','/infra-database/list','/infra-ftp/list','/infra-project-instance/list','/server','/domain'); 'infra-database'=@('/infra-database/list'); 'infra-ftp'=@('/infra-ftp/list','/ftp-list'); 'infra-project-instance'=@('/infra-project-instance/list');
  'inventory'=@('/inventory'); 'issue'=@('/issue'); 'job-post'=@('/job-post'); 'lead'=@('/lead'); 'leave-rules'=@('/leave-rules'); 'leave'=@('/apply-leave','/approve-leaves','/forward-leaves'); 'marketing-ad-post-channel'=@('/marketing-ad-post-channel'); 'marketing-ad-post'=@('/marketing-ad-post');
  'module'=@('/modules'); 'my-daily-planner'=@('/my-daily-planner'); 'person'=@('/person'); 'project-action-items'=$work; 'project-center'=$work; 'project-modules'=$work; 'project-planning'=$work; 'project-questions-answers'=$work; 'project-release-tracking'=$work; 'project-targetplan'=$work; 'project-tasks-activities'=$work; 'project-tasks'=$work; 'project'=$work;
  'reports'=@('/reports'); 'requirement-center'=(@('/requirement') + $work); 'requirement-data-mapping'=@('/requirement'); 'requirement-group'=@('/requirement-group','/requirement'); 'requirement-status-tracking'=@('/requirement'); 'requirement'=@('/requirement'); 'roles'=@('/site-roles','/master-roles','/sites');
  'sites'=@('/sites','/site-roles'); 'sop-assignment'=@('/sop-assignment'); 'sop-process'=@('/sop-process'); 'sop-template'=@('/sop-template'); 'tags'=@('/tags'); 'test-case'=@('/test-case'); 'test-plan'=@('/test-plan','/test-case'); 'timesheet'=@('/timesheet','/billing-processing-employee'); 'training-portal'=@('/training-portal'); 'user-management'=@('/user-management','/sites')
}
# A page whose own menu is narrower than its module's default.
$pathOverrides = @{
  '/lead-group-user-assignment/list'=@('/lead-group-user-assignment/list','/lead'); '/leave-credit'=@('/leave-credit'); '/project/assign-users-to-project'=@('/project/assign-users-to-project');
  '/master-roles'=@('/master-roles','/sites')
}

function PermissionText([string[]]$links) {
  $quoted = ($links | ForEach-Object { '"' + $_ + '"' }) -join ', '
  return "permission: { menu: [$quoted], level: `"view`" }"
}

$changed = 0
Get-ChildItem $ModulesPath -Directory | ForEach-Object {
  $module = $_.Name
  $file = Join-Path $_.FullName 'routes.js'
  if (-not (Test-Path $file) -or $module -eq 'common') { return }

  $bytes = [IO.File]::ReadAllBytes($file)
  $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
  $text = [Text.UTF8Encoding]::new($false).GetString($bytes)
  if ($hasBom) { $text = $text.Substring(1) }
  $eol = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
  $lines = $text -split "\r?\n"

  for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    if ($line -match '^\s*//' -or $line -match 'permission:|siteMember:|public:') { continue }
    if ($line -notmatch 'meta:\s*\{') { continue }

    $path = if ($line -match 'path:\s*"([^"]*)"') { $Matches[1] } else { '' }

    if ($public -contains $module) {
      $rule = 'public: true'
    } elseif ($siteMember -contains $module) {
      $rule = 'siteMember: true'
    } elseif ($pathOverrides.ContainsKey($path)) {
      $rule = PermissionText $pathOverrides[$path]
    } elseif ($menus.ContainsKey($module)) {
      $rule = PermissionText $menus[$module]
    } else {
      throw "No rule for module $module"
    }

    # Protected routes always require login, including the two that were missing it.
    $hasAuth = $line -match 'requiresAuth:\s*true'
    if ($public -contains $module) {
      $lines[$i] = $line -replace 'meta:\s*\{\s*', "meta: { $rule, "
    } elseif ($hasAuth) {
      $lines[$i] = $line -replace 'requiresAuth:\s*true,\s*', "requiresAuth: true, $rule, "
    } else {
      $lines[$i] = $line -replace 'meta:\s*\{\s*', "meta: { requiresAuth: true, $rule, "
    }
    $changed++
  }

  [IO.File]::WriteAllText($file, ($lines -join $eol), [Text.UTF8Encoding]::new($hasBom))
}
"Routes updated: $changed"
