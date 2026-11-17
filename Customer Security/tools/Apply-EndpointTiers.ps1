# One-time tool used in Phase 2.6. Adds authorization attributes to every controller, keeping each
# file's encoding and line endings. It only adds attribute lines, and is safe to re-run.
param([string]$ControllersPath)

$MenuClass = @{
  AdPostChannel='AdPostChannel'; AdPost='AdPost'; AllProjectPlanner='AllProjectPlanner'; Candidate='Candidate';
  CompanyContact='CompanyContacts + "|" + MenuLinks.Company'; Company='Company + "|" + MenuLinks.CompanyContacts + "|" + MenuLinks.Customer';
  Customer='Customer'; CustomerFiles='CustomerFiles + "|" + MenuLinks.Customer'; DailyPlanner='DailyPlanner'; Departments='Department'; Domain='Domain';
  DropDownTypes='DropdownTypes'; DropDowns='Dropdowns + "|" + MenuLinks.DropdownTypes'; EmployeeLeave='LeaveAny'; EmployeeOrgStructures='OrgStructure'; Employees='Employee';
  ExpenseAdvanceRequest='AdvanceExpenseAny'; ExpenseBankAccount='BankAccounts'; Expense='ExpenseAny'; ExpenseEmployee='ExpenseAny';
  ExpensePurchaseRequest='PurchaseExpenseAny'; ExpenseVendorBankAccounts='Vendors'; InfraAccount='InfraAccount'; InfraAccountServices='InfraAccountServices';
  InfraDashboard='InfraAny'; InfraDatabase='InfraDatabase'; InfraFTP='InfraFtp + "|" + MenuLinks.FtpList'; InfraProjectInstance='InfraProjectInstance'; Inventory='Inventory';
  Issue='Issue'; JobCreate='JobPost'; LeadUserGroupMapping='LeadGroupUsers + "|" + MenuLinks.Lead'; Leads='Lead'; LeaveCredit='LeaveCredit'; LeaveRules='LeaveRules'; Person='Person';
  ProjectActionItems='ProjectWork'; ProjectActivities='ProjectWork'; ProjectDashboard='ProjectWork'; ProjectModules='ProjectWork'; ProjectModulesUserMapping='ProjectWork';
  ProjectQuestionsAnswers='ProjectWork'; ProjectReleaseTracking='ProjectWork'; ProjectSwimlane='ProjectWork + "|" + MenuLinks.SwimlaneTemplates'; ProjectTasks='ProjectWork';
  ProjectUserMapping='ProjectSecurity'; Projects='ProjectWork'; ReportUserMapping='Reports'; Reports='Reports'; Requirement='Requirement';
  RequirementDashboard='Requirement + "|" + MenuLinks.ProjectWork'; RequirementDataMapping='Requirement'; RequirementGroup='RequirementGroup + "|" + MenuLinks.Requirement'; RequirementStatusTracking='Requirement';
  Roles='SiteRoles + "|" + MenuLinks.MasterRoles + "|" + MenuLinks.Sites'; SOPAssignment='SopAssignment'; SOPProcess='SopProcess'; SOPTemplate='SopTemplate'; SalesPerson='Lead';
  Server='Server'; SiteProjectRoles='ProjectWork + "|" + MenuLinks.SiteRoles'; Tags='Tags'; TestCase='TestCase'; TestPlan='TestPlan + "|" + MenuLinks.TestCase';
  TimesheetAISummary='Timesheet + "|" + MenuLinks.BillingProcessing'; Timesheet='Timesheet + "|" + MenuLinks.BillingProcessing'; TrainingPortal='TrainingPortal';
  Users='UserManagement + "|" + MenuLinks.Sites'
}
# These pages have no menu row, so the most that can be required is membership of the site.
$OtherClass = @{
  Account='SiteMember'; Common='SiteMember'; Contact='SiteMember'; EmailNotification='SiteMember'; HelpDesk='SiteMember'; HelpDeskTopicsQuestions='SiteMember';
  ItemCategory='SiteMember'; ItemSubCategoryAttributes='SiteMember'; LeaveSchedule='SiteMember'; Modules='SiteMember'; ModulesMenus='SiteMember'; MovementRegister='SiteMember';
  Notes='SiteMember'; Notification='SiteMember'; PersonSitesMapping='SiteMember'; Site='SiteMember'; SitesItemSubCategoryAttributesMapping='SiteMember'; SitesItems='SiteMember';
  TimeInTimeOut='SiteMember'; WebsiteDemos='SiteMember'; Calendar='AuthenticatedOnly'; Cronjob='ServiceKey'
}
# Lookups these controllers serve to many other pages.
$SharedReads = @('DropDowns','DropDownTypes','Tags')
$SiteOwned = @{
  AdPost='AdPost'; AdPostChannel='AdPostChannel'; Candidate='Candidate'; Company='Company'; CustomerFiles='CustomerFiles'; DailyPlanner='DailyPlanner';
  Departments='Department'; Domain='Domain'; EmployeeOrgStructures='EmployeeOrgStructure'; Employees='Employee'; ExpenseBankAccount='Expense_BankAccounts'; ExpenseVendorBankAccounts='ExpenseVendors';
  HelpDesk='HelpDesk'; HelpDeskTopicsQuestions='HelpDeskTopic'; InfraAccount='InfraAccount'; InfraDatabase='InfraDatabase'; InfraFTP='InfraFTP'; InfraProjectInstance='InfraProjectInstance';
  Inventory='Inventory'; Issue='Issue'; JobCreate='JobCreate'; Leads='Lead'; LeadUserGroupMapping='LeadUserGroupMapping'; LeaveRules='LeaveRules'; LeaveSchedule='LeaveSchedules';
  ProjectActionItems='ProjectActionItems'; ProjectActivities='ProjectActivity'; ProjectModules='ProjectModule'; ProjectQuestionsAnswers='ProjectQuestionsAnswers'; ProjectReleaseTracking='ProjectReleaseTracking';
  ProjectTasks='ProjectTask'; Projects='Project'; Requirement='Requirement'; SOPAssignment='SOPAssignment'; SOPProcess='SOPProcess'; SOPTemplate='SOPTemplate'; Server='Server'; SitesItems='SitesItems';
  Tags='Tags'; TestCase='TestCase'; TestPlan='TestPlan'; TimeInTimeOut='TimeInTimeOut'; Timesheet='Timesheet'; TrainingPortal='TrainingPortal'
}
# Only modules whose list uses this same project rule. ProjectActivities uses a different rule.
$RecordScope = @{ RequirementGroup='RequirementGroup'; TestCase='TestCase'; Issue='Issue'; Requirement='Requirement'; TestPlan='TestPlan'; ProjectModules='ProjectModule'; ProjectActionItems='ProjectActionItems';
  ProjectQuestionsAnswers='ProjectQuestionsAnswers'; ProjectReleaseTracking='ProjectReleaseTracking'; ProjectTasks='ProjectTask' }
$RuleScope = @{ Projects='Project'; Leads='Lead'; HelpDesk='HelpDesk'; SOPProcess='SopProcess'; EmployeeLeave='Leave' }
$TierPattern = 'AllowAnonymous|RequireSiteAdmin|RequireSystemAdmin|AuthenticatedOnly|\[SiteMember|RequireMenu|ServiceKey'

$added = 0
Get-ChildItem $ControllersPath -Filter *Controller.cs | ForEach-Object {
  $name = $_.BaseName -replace 'Controller$',''
  if ($name -in 'Base','Auth','Authorization') { return }

  $bytes = [IO.File]::ReadAllBytes($_.FullName)
  $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
  $text = [Text.UTF8Encoding]::new($false).GetString($bytes)
  if ($hasBom) { $text = $text.Substring(1) }
  $eol = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
  $lines = [Collections.Generic.List[string]]::new()
  $lines.AddRange([string[]]($text -split "\r?\n"))

  $classAttr = if ($MenuClass.ContainsKey($name)) { "[RequireMenu(MenuLinks.$($MenuClass[$name]))]" }
               elseif ($OtherClass.ContainsKey($name)) { "[$($OtherClass[$name])]" }
               else { throw "No rule for controller $name" }
  $isMenuClass = $MenuClass.ContainsKey($name)

  for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -notmatch '^\s*\[Http(Get|Post|Put|Delete)(\("([^"]*)"\))?') { continue }
    $verb = $Matches[1]; $route = "$($Matches[3])"
    $indent = ($lines[$i] -replace '\S.*$','')

    $j = $i + 1
    while ($j -lt $lines.Count -and $lines[$j] -notmatch '^\s*public ') { $j++ }
    if ($j -ge $lines.Count) { continue }
    $method = (($lines[$j] -replace '\(.*$','') -split '\s+')[-1]

    $start = $i
    while ($start -gt 0 -and $lines[$start - 1] -match '^\s*\[') { $start-- }
    $block = ($lines[$start..($j - 1)] -join ' ')
    $hasTier = $block -match $TierPattern
    $isAnon = $block -match 'AllowAnonymous'

    $isRead = $verb -eq 'Get' -or ($verb -eq 'Post' -and ($route -match '(?i)list' -or $method -match '^(Get|Check|Export|Popup)'))
    $isLookup = $verb -eq 'Get' -and $route -match '(?i)drop-?down|list'
    $isWidget = "$route $method" -match '(?i)dashboard' -and $name -notin 'ProjectDashboard','RequirementDashboard','InfraDashboard'

    $new = @()
    if ($verb -eq 'Post' -and $isRead -and $block -notmatch 'ReadAccess') { $new += '[ReadAccess]' }
    if (-not $hasTier -and $isMenuClass) {
      if ($isWidget) { $new += "[RequireMenu(MenuLinks.Dashboard + `"|`" + MenuLinks.$($MenuClass[$name]))]" }
      elseif ($isLookup -or ($name -in $SharedReads -and $isRead)) { $new += '[SiteMember]' }
    }
    if ($route -match '^(details/)?\{id\}$' -and -not $isAnon) {
      if ($SiteOwned.ContainsKey($name) -and $block -notmatch 'SiteOwned') { $new += "[SiteOwned(typeof(Vsky.Models.$($SiteOwned[$name])))]" }
      if ($RuleScope.ContainsKey($name) -and $block -notmatch 'RecordScope') { $new += "[RecordScope(Vsky.Services.Authorization.RecordRule.$($RuleScope[$name]))]" }
      if ($RecordScope.ContainsKey($name) -and $block -notmatch 'RecordScope') { $new += "[RecordScope(typeof(Vsky.Models.$($RecordScope[$name])))]" }
    }

    for ($k = $new.Count - 1; $k -ge 0; $k--) { $lines.Insert($i + 1, $indent + $new[$k]); $added++ }
    $i = $j + $new.Count
  }

  $alreadyHasClassTier = $false
  for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match "^\s*public class ${name}Controller\b") {
      $s = $i
      while ($s -gt 0 -and $lines[$s - 1] -match '^\s*(\[|//)') {
        if ($lines[$s - 1] -match 'AuthenticatedOnly|\[SiteMember|RequireMenu|ServiceKey') { $alreadyHasClassTier = $true }
        $s--
      }
      if (-not $alreadyHasClassTier) {
        $indent = ($lines[$i] -replace '\S.*$','')
        $lines.Insert($i, $indent + $classAttr); $added++
      }
      break
    }
  }

  $output = ($lines -join $eol)
  [IO.File]::WriteAllText($_.FullName, $output, [Text.UTF8Encoding]::new($hasBom))
}
"Attributes added: $added"
