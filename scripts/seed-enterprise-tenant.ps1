param (
    [string]$Email = "alex@apexdynamics.com",
    [string]$Password = "Password123!",
    [string]$BaseUrl = "http://localhost:5000/api/v1"
)

$ErrorActionPreference = "Continue"

Write-Host "Starting FusionOS Enterprise Tenant Seeding..."

# ------------------------------------------------------------------------------
# 1. Login to obtain JWT Bearer token
# ------------------------------------------------------------------------------
Write-Host "Authenticating with $Email..."
$loginBody = @{
    email = $Email
    password = $Password
} | ConvertTo-Json

try {
    $authRes = Invoke-RestMethod -Uri "$BaseUrl/core/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
    $token = $authRes.accessToken
    $companyId = $authRes.companyId
    Write-Host "Authenticated successfully! CompanyId: $companyId" -ForegroundColor Green
} catch {
    Write-Host "Login failed: $_. Falling back to registration..." -ForegroundColor Yellow

    # Register Company & Owner User
    $regBody = @{
        name = "Apex Dynamics Corp"
        legalName = "Apex Dynamics Corp"
        baseCurrency = "USD"
        ownerEmail = $Email
        ownerPassword = $Password
        ownerFullName = "Alex Mercer"
    } | ConvertTo-Json

    try {
        $compRes = Invoke-RestMethod -Uri "$BaseUrl/core/companies" -Method Post -ContentType "application/json" -Body $regBody
        $authRes = Invoke-RestMethod -Uri "$BaseUrl/core/auth/login" -Method Post -ContentType "application/json" -Body $loginBody
        $token = $authRes.accessToken
        $companyId = $authRes.companyId
        Write-Host "Registered and authenticated! CompanyId: $companyId" -ForegroundColor Green
    } catch {
        Write-Host "Failed to register/authenticate: $_" -ForegroundColor Red
        exit 1
    }
}

$headers = @{
    Authorization = "Bearer $token"
}

# ------------------------------------------------------------------------------
# 2. Seed 22 Products
# ------------------------------------------------------------------------------
$products = @(
    @{ Sku = "ROB-ARM-6AX"; Name = "6-Axis Industrial Robotic Arm"; Uom = "SET"; Description = "High-precision 6-axis articulated arm for automated assembly line." },
    @{ Sku = "ROB-AGV-500"; Name = "Automated Guided Vehicle 500kg"; Uom = "UNIT"; Description = "Self-navigating autonomous mobile robot with 500kg payload capacity." },
    @{ Sku = "ROB-SRV-400"; Name = "High-Torque Servo Motor 400W"; Uom = "PCS"; Description = "Brushless AC servo motor with absolute encoder feedback." },
    @{ Sku = "ROB-GRIP-2F"; Name = "Dual-Finger Electric Gripper"; Uom = "PCS"; Description = "Programmable force-controlled parallel gripper." },

    @{ Sku = "ELE-PLC-SR8"; Name = "Industrial PLC Controller 8-CH"; Uom = "PCS"; Description = "Modular programmable logic controller with Modbus/TCP." },
    @{ Sku = "ELE-HMI-10"; Name = "10.1 Multi-Touch HMI Panel"; Uom = "PCS"; Description = "IP65 widescreen capacitive touch human-machine interface." },
    @{ Sku = "ELE-DRV-10K"; Name = "Variable Frequency Drive 10kW"; Uom = "PCS"; Description = "Heavy-duty 3-phase motor inverter drive with STO safety." },
    @{ Sku = "ELE-SNSR-OPT"; Name = "Optical Laser Distance Sensor"; Uom = "PCS"; Description = "Sub-millimeter precision time-of-flight optical sensor." },

    @{ Sku = "PWR-PSU-48V"; Name = "48V 30A Industrial Power Supply"; Uom = "PCS"; Description = "DIN-rail mounted switch-mode power supply 1440W." },
    @{ Sku = "PWR-BATT-48V"; Name = "Lithium Iron Battery Pack 48V"; Uom = "PACK"; Description = "LiFePO4 high-energy density module 100Ah with BMS." },
    @{ Sku = "PWR-INV-5KW"; Name = "Pure Sine Wave Inverter 5kW"; Uom = "PCS"; Description = "Industrial grade DC-AC inverter 48VDC to 230VAC." },

    @{ Sku = "HYD-CYL-100"; Name = "Heavy-Duty Hydraulic Cylinder"; Uom = "PCS"; Description = "Double-acting hydraulic actuator 100mm bore x 500mm stroke." },
    @{ Sku = "HYD-PUMP-25"; Name = "Hydraulic Gear Pump 25L/min"; Uom = "PCS"; Description = "Positive displacement external gear pump 210 bar." },
    @{ Sku = "HYD-VALV-4W"; Name = "4-Way Solenoid Directional Valve"; Uom = "PCS"; Description = "CETOP 3 manifold mounting directional control valve." },

    @{ Sku = "RAW-ALU-6061"; Name = "Aluminum Alloy Ingot 6061-T6"; Uom = "KG"; Description = "Structural aircraft-grade aluminum alloy stock material." },
    @{ Sku = "RAW-STL-316"; Name = "Stainless Steel Bar 316L"; Uom = "METER"; Description = "Corrosion-resistant austenitic stainless steel rod 50mm." },
    @{ Sku = "RAW-COP-WIRE"; Name = "Enamelled Copper Magnet Wire"; Uom = "REEL"; Description = "High-temperature grade 200C copper winding wire." },
    @{ Sku = "RAW-PCB-4L"; Name = "4-Layer FR4 Bare PCB Substrate"; Uom = "PCS"; Description = "High-TG impedance controlled bare printed circuit board." },

    @{ Sku = "HDW-SCR-M6"; Name = "Stainless Steel Hex Bolt M6x20"; Uom = "BOX"; Description = "Pack of 100 A2-70 grade stainless steel bolts." },
    @{ Sku = "HDW-BRG-6204"; Name = "Deep Groove Ball Bearing 6204"; Uom = "PCS"; Description = "Precision rubber-sealed ball bearing 20x47x14mm." },
    @{ Sku = "HDW-SEAL-O30"; Name = "Viton O-Ring Seal Kit 30mm"; Uom = "SET"; Description = "High-chemical resistance fluorocarbon sealing rings." },
    @{ Sku = "HDW-CABL-CAT6"; Name = "Shielded Industrial Ethernet Cable"; Uom = "METER"; Description = "Cat6A S/FTP double-shielded PUR jacket cable." }
)

Write-Host "Seeding Products..."
foreach ($p in $products) {
    $body = @{
        companyId = $companyId
        sku = $p.Sku
        name = $p.Name
        unitOfMeasure = $p.Uom
        description = $p.Description
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/inventory/products" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        Write-Host "  [OK] Product created: $($p.Sku) - $($p.Name)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Product $($p.Sku): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

# ------------------------------------------------------------------------------
# 3. Seed Warehouses (Warehouse & Inventory Logistics)
# ------------------------------------------------------------------------------
$warehouses = @(
    @{ Code = "WH-MAIN-01"; Name = "Main Assembly Plant"; Address = "1 Robotics Way, Fremont, CA" },
    @{ Code = "WH-COMP-01"; Name = "Raw Component Storage"; Address = "2 Component Blvd, Fremont, CA" }
)

$warehouseIds = @{}
Write-Host "Seeding Warehouses..."
foreach ($w in $warehouses) {
    $body = @{
        companyId = $companyId
        code = $w.Code
        name = $w.Name
        address = $w.Address
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/warehouse/warehouses" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        $warehouseIds[$w.Code] = $res.id
        Write-Host "  [OK] Warehouse created: $($w.Code) - $($w.Name)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Warehouse $($w.Code): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

# Fall back to looking up warehouse ids if creation reported "already exists"
if ($warehouseIds.Count -lt $warehouses.Count) {
    try {
        $whRes = Invoke-RestMethod -Uri "$BaseUrl/warehouse/warehouses?companyId=$companyId&pageSize=100" -Headers $headers
        foreach ($w in $warehouses) {
            if (-not $warehouseIds.ContainsKey($w.Code)) {
                $match = $whRes.data | Where-Object { $_.code -eq $w.Code }
                if ($match) { $warehouseIds[$w.Code] = $match.id }
            }
        }
    } catch {
        Write-Host "  [INFO] Warehouse lookup: $_" -ForegroundColor Gray
    }
}

# ------------------------------------------------------------------------------
# 4. Seed Work Centers
# ------------------------------------------------------------------------------
$workCenters = @(
    @{ Code = "WC-CNC-01"; Name = "CNC Precision Machining Center"; Capacity = 16.0 },
    @{ Code = "WC-SMT-01"; Name = "Surface Mount Technology PCB Assembly Line"; Capacity = 24.0 },
    @{ Code = "WC-ASY-01"; Name = "Robotic Main Assembly Bench"; Capacity = 16.0 },
    @{ Code = "WC-TST-01"; Name = "Quality Inspection and Calibration Station"; Capacity = 8.0 }
)

Write-Host "Seeding Work Centers..."
foreach ($wc in $workCenters) {
    $body = @{
        companyId = $companyId
        code = $wc.Code
        name = $wc.Name
        capacityHoursPerDay = $wc.Capacity
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/manufacturing/work-centers" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        Write-Host "  [OK] Work Center created: $($wc.Code) - $($wc.Name)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Work Center $($wc.Code): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

# ------------------------------------------------------------------------------
# 5. Seed Cost Centers
# ------------------------------------------------------------------------------
$costCenters = @(
    @{ Code = "CC-PROD-01"; Name = "Manufacturing Operations" },
    @{ Code = "CC-RND-01"; Name = "Robotics Research and Development" },
    @{ Code = "CC-SALES-01"; Name = "Global Sales and Marketing" }
)

$costCenterIds = @{}
Write-Host "Seeding Cost Centers..."
foreach ($cc in $costCenters) {
    $body = @{
        companyId = $companyId
        code = $cc.Code
        name = $cc.Name
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/finance/cost-centers" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        $costCenterIds[$cc.Code] = $res.id
        Write-Host "  [OK] Cost Center created: $($cc.Code) - $($cc.Name)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Cost Center $($cc.Code): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

if ($costCenterIds.Count -lt $costCenters.Count) {
    try {
        $ccRes = Invoke-RestMethod -Uri "$BaseUrl/finance/cost-centers?companyId=$companyId&pageSize=100" -Headers $headers
        foreach ($cc in $costCenters) {
            if (-not $costCenterIds.ContainsKey($cc.Code)) {
                $match = $ccRes.data | Where-Object { $_.code -eq $cc.Code }
                if ($match) { $costCenterIds[$cc.Code] = $match.id }
            }
        }
    } catch {
        Write-Host "  [INFO] Cost Center lookup: $_" -ForegroundColor Gray
    }
}

# ------------------------------------------------------------------------------
# 6. Seed Financial General Ledger Accounts
# ------------------------------------------------------------------------------
$accounts = @(
    @{ Code = "1010"; Name = "Operating Cash and Bank Account"; Type = "Asset" },
    @{ Code = "1200"; Name = "Accounts Receivable"; Type = "Asset" },
    @{ Code = "1400"; Name = "Raw Material Inventory"; Type = "Asset" },
    @{ Code = "2000"; Name = "Accounts Payable"; Type = "Liability" },
    @{ Code = "4000"; Name = "Industrial Product Sales Revenue"; Type = "Revenue" },
    @{ Code = "5000"; Name = "Cost of Goods Sold - Materials"; Type = "Expense" }
)

Write-Host "Seeding Financial Accounts..."
foreach ($a in $accounts) {
    $body = @{
        companyId = $companyId
        code = $a.Code
        name = $a.Name
        accountType = $a.Type
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/finance/accounts" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        Write-Host "  [OK] Account created: $($a.Code) - $($a.Name)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Account $($a.Code): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

# ------------------------------------------------------------------------------
# 7. Look up product ids needed by later steps
# ------------------------------------------------------------------------------
$prodsRes = $null
try {
    $prodsRes = Invoke-RestMethod -Uri "$BaseUrl/inventory/products?companyId=$companyId&pageSize=100" -Headers $headers
} catch {
    Write-Host "  [INFO] Product lookup: $_" -ForegroundColor Gray
}
$armProd = $prodsRes.data | Where-Object { $_.sku -eq "ROB-ARM-6AX" }
$srvProd = $prodsRes.data | Where-Object { $_.sku -eq "ROB-SRV-400" }
$plcProd = $prodsRes.data | Where-Object { $_.sku -eq "ELE-PLC-SR8" }
$gripProd = $prodsRes.data | Where-Object { $_.sku -eq "ROB-GRIP-2F" }
$aluProd = $prodsRes.data | Where-Object { $_.sku -eq "RAW-ALU-6061" }
$brgProd = $prodsRes.data | Where-Object { $_.sku -eq "HDW-BRG-6204" }

# ------------------------------------------------------------------------------
# 8. Seed Multi-Level Bill of Materials (BOM) for ROB-ARM-6AX (5 components)
# ------------------------------------------------------------------------------
Write-Host "Seeding Bills of Materials (BOM)..."
$bomId = $null
try {
    if ($armProd -and $srvProd -and $plcProd -and $gripProd -and $aluProd -and $brgProd) {
        $bomBody = @{
            companyId = $companyId
            code = "BOM-ROB-ARM-6AX"
            name = "6-Axis Robotic Arm Master Assembly"
            productId = $armProd.id
            lines = @(
                @{ componentProductId = $srvProd.id; quantity = 4.0; unitOfMeasure = "PCS" },
                @{ componentProductId = $plcProd.id; quantity = 1.0; unitOfMeasure = "PCS" },
                @{ componentProductId = $gripProd.id; quantity = 1.0; unitOfMeasure = "PCS" },
                @{ componentProductId = $aluProd.id; quantity = 12.0; unitOfMeasure = "KG" },
                @{ componentProductId = $brgProd.id; quantity = 6.0; unitOfMeasure = "PCS" }
            )
        } | ConvertTo-Json -Depth 5

        $bomRes = Invoke-RestMethod -Uri "$BaseUrl/manufacturing/bills-of-materials" -Method Post -Headers $headers -ContentType "application/json" -Body $bomBody
        $bomId = $bomRes.id
        Write-Host "  [OK] Bill of Materials created: BOM-ROB-ARM-6AX (5 components)" -ForegroundColor Green
    } else {
        Write-Host "  [INFO] BOM skipped - one or more component products not found." -ForegroundColor Gray
    }
} catch {
    Write-Host "  [INFO] BOM creation: $_" -ForegroundColor Gray
}

if (-not $bomId) {
    try {
        $bomsRes = Invoke-RestMethod -Uri "$BaseUrl/manufacturing/bills-of-materials?companyId=$companyId&pageSize=100" -Headers $headers
        $match = $bomsRes.data | Where-Object { $_.code -eq "BOM-ROB-ARM-6AX" }
        if ($match) { $bomId = $match.id }
    } catch {
        Write-Host "  [INFO] BOM lookup: $_" -ForegroundColor Gray
    }
}

# ------------------------------------------------------------------------------
# 9. Seed Work Order for ROB-ARM-6AX assembly
# ------------------------------------------------------------------------------
# NOTE: CreateWorkOrderCommand does not accept a custom code/reference or a
# work center - the work order number is system-generated, and the target
# work center (WC-ASY-01) is assigned separately via the operation-scheduling
# endpoints, not at creation time.
Write-Host "Seeding Work Order (Assembly)..."
$mainWarehouseId = $warehouseIds["WH-MAIN-01"]
if ($bomId -and $mainWarehouseId) {
    try {
        $woBody = @{
            companyId = $companyId
            billOfMaterialsId = $bomId
            warehouseId = $mainWarehouseId
            quantityToProduce = 10.0
        } | ConvertTo-Json

        $woRes = Invoke-RestMethod -Uri "$BaseUrl/manufacturing/work-orders" -Method Post -Headers $headers -ContentType "application/json" -Body $woBody
        Write-Host "  [OK] Work Order created: $($woRes.id) - 10x ROB-ARM-6AX at WH-MAIN-01" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Work Order creation: $_" -ForegroundColor Gray
    }
} else {
    Write-Host "  [INFO] Work Order skipped - BOM or Main Warehouse not available." -ForegroundColor Gray
}

# ------------------------------------------------------------------------------
# 10. Seed CRM/Sales B2B Customers
# ------------------------------------------------------------------------------
$customers = @(
    @{ Code = "CUST-TESLA"; Name = "Tesla Gigafactory"; Email = "procurement@tesla-gigafactory.example.com" },
    @{ Code = "CUST-SIEMENS"; Name = "Siemens Automation"; Email = "procurement@siemens-automation.example.com" },
    @{ Code = "CUST-ABB"; Name = "ABB Robotics"; Email = "procurement@abb-robotics.example.com" }
)

$customerIds = @{}
Write-Host "Seeding B2B Customers..."
foreach ($c in $customers) {
    $body = @{
        companyId = $companyId
        code = $c.Code
        name = $c.Name
        contactEmail = $c.Email
        creditLimit = 500000.00
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/sales/customers" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        $customerIds[$c.Code] = $res.id
        Write-Host "  [OK] Customer created: $($c.Code) - $($c.Name)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Customer $($c.Code): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

if ($customerIds.Count -lt $customers.Count) {
    try {
        $custRes = Invoke-RestMethod -Uri "$BaseUrl/sales/customers?companyId=$companyId&pageSize=100" -Headers $headers
        foreach ($c in $customers) {
            if (-not $customerIds.ContainsKey($c.Code)) {
                $match = $custRes.data | Where-Object { $_.code -eq $c.Code }
                if ($match) { $customerIds[$c.Code] = $match.id }
            }
        }
    } catch {
        Write-Host "  [INFO] Customer lookup: $_" -ForegroundColor Gray
    }
}

# ------------------------------------------------------------------------------
# 11. Seed finished-goods stock at WH-MAIN-01 so the Sales Order below has
#     real, reservable on-hand inventory (simulates completed production).
# ------------------------------------------------------------------------------
Write-Host "Adjusting finished-goods stock for reservation..."
if ($armProd -and $mainWarehouseId) {
    try {
        $adjBody = @{
            companyId = $companyId
            productId = $armProd.id
            warehouseId = $mainWarehouseId
            quantityDelta = 20.0
            reason = "Initial finished-goods stock from completed robotic arm production run"
        } | ConvertTo-Json

        $adjRes = Invoke-RestMethod -Uri "$BaseUrl/inventory/stock/adjustments" -Method Post -Headers $headers -ContentType "application/json" -Body $adjBody
        Write-Host "  [OK] Stock adjusted: +20 ROB-ARM-6AX at WH-MAIN-01" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Stock adjustment: $_" -ForegroundColor Gray
    }
}

# ------------------------------------------------------------------------------
# 12. Seed Sales Order: 10x ROB-ARM-6AX ($145,000.00) to Tesla Gigafactory,
#     then confirm it, then reserve stock against WH-MAIN-01.
# ------------------------------------------------------------------------------
Write-Host "Seeding Sales Order..."
$teslaId = $customerIds["CUST-TESLA"]
if ($teslaId -and $armProd) {
    try {
        $soBody = @{
            companyId = $companyId
            customerId = $teslaId
            lines = @(
                @{ productId = $armProd.id; quantity = 10.0; unitPrice = 14500.00; discountPercentage = 0.0 }
            )
        } | ConvertTo-Json -Depth 5

        $soRes = Invoke-RestMethod -Uri "$BaseUrl/sales/sales-orders" -Method Post -Headers $headers -ContentType "application/json" -Body $soBody
        Write-Host "  [OK] Sales Order created: $($soRes.id) - 10x ROB-ARM-6AX = `$145,000.00 to Tesla Gigafactory" -ForegroundColor Green

        try {
            # companyId must be a query parameter here, not a body field - this
            # controller action binds it via [FromQuery], same convention as
            # Procurement's PurchaseOrder Approve/Cancel actions. Sending it in
            # the body leaves the bound value as Guid.Empty and the tenant
            # isolation check rejects the request with 403.
            $confirmRes = Invoke-RestMethod -Uri "$BaseUrl/sales/sales-orders/$($soRes.id)/confirm?companyId=$companyId" -Method Post -Headers $headers -ContentType "application/json" -Body "{}"
            Write-Host "  [OK] Sales Order confirmed: $($soRes.id)" -ForegroundColor Green
        } catch {
            Write-Host "  [INFO] Sales Order confirm: $_" -ForegroundColor Gray
        }

        try {
            $reserveBody = @{
                companyId = $companyId
                productId = $armProd.id
                warehouseId = $mainWarehouseId
                quantity = 10.0
                referenceType = "SalesOrder"
                referenceId = $soRes.id
            } | ConvertTo-Json

            $reserveRes = Invoke-RestMethod -Uri "$BaseUrl/inventory/reservations" -Method Post -Headers $headers -ContentType "application/json" -Body $reserveBody
            Write-Host "  [OK] Stock reserved: 10x ROB-ARM-6AX at WH-MAIN-01 against Sales Order $($soRes.id)" -ForegroundColor Green
        } catch {
            Write-Host "  [INFO] Stock reservation: $_" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  [INFO] Sales Order creation: $_" -ForegroundColor Gray
    }
} else {
    Write-Host "  [INFO] Sales Order skipped - Tesla customer or ROB-ARM-6AX product not available." -ForegroundColor Gray
}

# ------------------------------------------------------------------------------
# 13. Seed Departments (Core module - HRMS roster organization)
# ------------------------------------------------------------------------------
$departments = @(
    @{ Code = "DEPT-RND"; Name = "Research and Development" },
    @{ Code = "DEPT-ASY"; Name = "Assembly" },
    @{ Code = "DEPT-QA"; Name = "Quality" }
)

$departmentIds = @{}
Write-Host "Seeding Departments..."
foreach ($d in $departments) {
    $body = @{
        companyId = $companyId
        code = $d.Code
        name = $d.Name
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/core/departments" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        $departmentIds[$d.Code] = $res.id
        Write-Host "  [OK] Department created: $($d.Code) - $($d.Name)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Department $($d.Code): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

if ($departmentIds.Count -lt $departments.Count) {
    try {
        $deptRes = Invoke-RestMethod -Uri "$BaseUrl/core/departments?companyId=$companyId&pageSize=100" -Headers $headers
        foreach ($d in $departments) {
            if (-not $departmentIds.ContainsKey($d.Code)) {
                $match = $deptRes.data | Where-Object { $_.code -eq $d.Code }
                if ($match) { $departmentIds[$d.Code] = $match.id }
            }
        }
    } catch {
        Write-Host "  [INFO] Department lookup: $_" -ForegroundColor Gray
    }
}

# ------------------------------------------------------------------------------
# 14. Seed HRMS Employee Roster
# ------------------------------------------------------------------------------
# NOTE: CreateEmployeeCommand has no job-title/position field today - the role
# is conveyed only through fullName below, not a first-class attribute.
$employees = @(
    @{ Code = "EMP-RND-01"; FullName = "Dr. Elena Vasquez - Lead Roboticist"; Email = "elena.vasquez@apexdynamics.example.com"; DeptCode = "DEPT-RND" },
    @{ Code = "EMP-ASY-01"; FullName = "Marcus Chen - Production Manager"; Email = "marcus.chen@apexdynamics.example.com"; DeptCode = "DEPT-ASY" },
    @{ Code = "EMP-QA-01"; FullName = "Priya Nair - Quality Engineer"; Email = "priya.nair@apexdynamics.example.com"; DeptCode = "DEPT-QA" }
)

Write-Host "Seeding Employee Roster..."
$hireDate = (Get-Date).ToString("yyyy-MM-ddT00:00:00Z")
foreach ($e in $employees) {
    $deptId = $departmentIds[$e.DeptCode]
    $body = @{
        companyId = $companyId
        code = $e.Code
        fullName = $e.FullName
        email = $e.Email
        hireDate = $hireDate
        departmentId = $deptId
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/hrms/employees" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        Write-Host "  [OK] Employee created: $($e.Code) - $($e.FullName)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Employee $($e.Code): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

# ------------------------------------------------------------------------------
# 15. Seed Multidimensional Balanced Journal Entry
#     (Account + CostCenter + Department + Project tags on each line)
# ------------------------------------------------------------------------------
Write-Host "Seeding Multidimensional Journal Entry..."
# There is no first-class Project entity in Finance today - ProjectId on a
# journal line is an opaque, unvalidated Guid slot. We generate a stable id
# here to stand in for "Project Titan" so every line that should be tagged to
# it uses the same value.
$projectTitanId = [guid]::NewGuid().ToString()
Write-Host "  [INFO] Project Titan tag id (no dedicated Project entity exists yet): $projectTitanId" -ForegroundColor Gray

try {
    $acctsRes = Invoke-RestMethod -Uri "$BaseUrl/finance/accounts?companyId=$companyId&pageSize=100" -Headers $headers
    $cashAcct = $acctsRes.data | Where-Object { $_.code -eq "1010" }
    $invAcct = $acctsRes.data | Where-Object { $_.code -eq "1400" }
    $prodCC = $costCenterIds["CC-PROD-01"]
    $assemblyDept = $departmentIds["DEPT-ASY"]

    if ($cashAcct -and $invAcct) {
        $jeBody = @{
            companyId = $companyId
            reference = "JE-2026-RAW-MAT-PURCHASE"
            lines = @(
                @{
                    accountId = $invAcct.id
                    debit = 14500.00
                    credit = 0.00
                    description = "Raw Component Stock Deposit"
                    costCenterId = $prodCC
                    departmentId = $assemblyDept
                    projectId = $projectTitanId
                },
                @{
                    accountId = $cashAcct.id
                    debit = 0.00
                    credit = 14500.00
                    description = "Operating Cash Disbursement"
                    costCenterId = $prodCC
                    departmentId = $assemblyDept
                    projectId = $projectTitanId
                }
            )
        } | ConvertTo-Json -Depth 5

        $jeRes = Invoke-RestMethod -Uri "$BaseUrl/finance/journal-entries" -Method Post -Headers $headers -ContentType "application/json" -Body $jeBody
        Write-Host "  [OK] Journal Entry created: JE-2026-RAW-MAT-PURCHASE (`$14,500.00) tagged CC-PROD-01/DEPT-ASY/Project Titan" -ForegroundColor Green
    }
} catch {
    Write-Host "  [INFO] Journal Entry creation: $_" -ForegroundColor Gray
}

# ------------------------------------------------------------------------------
# 16. Seed Foreign Currency Exchange Rates + attempt FX revaluation
# ------------------------------------------------------------------------------
# NOTE ON FEASIBILITY: research against the actual backend source found that
# RevalueForeignCurrencyBalancesCommandHandler exists but is a stub (always
# revalues a hardcoded "USD" bucket, returns RevaluedAmount=0/GainLossAmount=0,
# and posts no real GL entries) and, more importantly, no REST controller
# route was found wired up to invoke it at all. The exchange-rate creation
# calls below are real and will succeed; the revaluation call is attempted
# anyway for completeness but is expected to 404 today - this is a genuine
# backend gap, not a bug in this script.
Write-Host "Seeding Foreign Currency Exchange Rates..."
$fxRates = @(
    @{ From = "EUR"; To = "USD"; Rate = 1.08 },
    @{ From = "JPY"; To = "USD"; Rate = 0.0067 }
)
$effectiveDate = (Get-Date).ToString("yyyy-MM-ddT00:00:00Z")
foreach ($fx in $fxRates) {
    $body = @{
        companyId = $companyId
        fromCurrencyCode = $fx.From
        toCurrencyCode = $fx.To
        rate = $fx.Rate
        effectiveDate = $effectiveDate
    } | ConvertTo-Json

    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/finance/exchange-rates" -Method Post -Headers $headers -ContentType "application/json" -Body $body
        Write-Host "  [OK] Exchange Rate created: $($fx.From) -> $($fx.To) @ $($fx.Rate)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Exchange Rate $($fx.From)->$($fx.To): $_" -ForegroundColor Gray
    }
    Start-Sleep -Milliseconds 100
}

Write-Host "Attempting Foreign Currency Balance Revaluation (EUR, JPY)..."
foreach ($fx in $fxRates) {
    $revalBody = @{
        companyId = $companyId
        currencyCode = $fx.From
        newExchangeRate = $fx.Rate
        effectiveDate = $effectiveDate
    } | ConvertTo-Json

    try {
        $revalRes = Invoke-RestMethod -Uri "$BaseUrl/finance/exchange-rates/revalue" -Method Post -Headers $headers -ContentType "application/json" -Body $revalBody
        Write-Host "  [OK] Revaluation executed for $($fx.From)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Revaluation for $($fx.From) - no REST endpoint is wired up yet for RevalueForeignCurrencyBalancesCommand (known backend gap): $_" -ForegroundColor Yellow
    }
    Start-Sleep -Milliseconds 100
}

# ------------------------------------------------------------------------------
# 17. Seed Quality Inspection for incoming ROB-SRV-400 Servo Motors
# ------------------------------------------------------------------------------
# NOTE: CreateInspectionCommand's referenceId is an opaque Guid (by convention
# tied to whatever business document triggered the inspection, e.g. a Goods
# Receipt line) and is not enforced against a real record server-side. We use
# the servo motor product's own id here as a stand-in reference rather than
# building out a full Supplier -> Purchase Order -> Goods Receipt chain first.
Write-Host "Seeding Quality Inspection..."
if ($srvProd) {
    try {
        # InspectionType has no [JsonStringEnumConverter] on the request DTO, so
        # System.Text.Json expects the underlying numeric value on the way in
        # even though responses serialize it back out as a string
        # (IncomingGoods=0, Production=1, Final=2) - a minor API asymmetry
        # worth flagging, not something this seeding script can fix.
        $inspBody = @{
            companyId = $companyId
            type = 0  # IncomingGoods
            referenceId = $srvProd.id
            characteristics = @("Torque Output", "Insulation Resistance", "Encoder Accuracy")
        } | ConvertTo-Json -Depth 5

        $inspRes = Invoke-RestMethod -Uri "$BaseUrl/quality/inspections" -Method Post -Headers $headers -ContentType "application/json" -Body $inspBody
        Write-Host "  [OK] Quality Inspection created for incoming ROB-SRV-400 Servo Motors: $($inspRes.id)" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Quality Inspection creation: $_" -ForegroundColor Gray
    }
} else {
    Write-Host "  [INFO] Quality Inspection skipped - ROB-SRV-400 product not available." -ForegroundColor Gray
}

# ------------------------------------------------------------------------------
# 18. Seed CNC Machine Maintenance Asset + Schedule
# ------------------------------------------------------------------------------
Write-Host "Seeding Maintenance Asset and Schedule..."
$assetId = $null
try {
    $assetBody = @{
        companyId = $companyId
        code = "CNC-MILL-01"
        name = "CNC Precision Milling Machine"
        location = "Plant Floor A - Machining Bay 1"
    } | ConvertTo-Json

    $assetRes = Invoke-RestMethod -Uri "$BaseUrl/maintenance/assets" -Method Post -Headers $headers -ContentType "application/json" -Body $assetBody
    $assetId = $assetRes.id
    Write-Host "  [OK] Maintenance Asset created: CNC-MILL-01 - CNC Precision Milling Machine" -ForegroundColor Green
} catch {
    Write-Host "  [INFO] Maintenance Asset creation: $_" -ForegroundColor Gray
}

if (-not $assetId) {
    try {
        $assetsRes = Invoke-RestMethod -Uri "$BaseUrl/maintenance/assets?companyId=$companyId&pageSize=100" -Headers $headers
        $match = $assetsRes.data | Where-Object { $_.code -eq "CNC-MILL-01" }
        if ($match) { $assetId = $match.id }
    } catch {
        Write-Host "  [INFO] Maintenance Asset lookup: $_" -ForegroundColor Gray
    }
}

if ($assetId) {
    try {
        # Same enum-as-integer asymmetry as InspectionType above:
        # MaintenanceScheduleFrequency (Weekly=0, Monthly=1, Quarterly=2, Annual=3)
        # has no string converter on the request DTO either.
        $nextDueDate = (Get-Date).AddDays(30).ToString("yyyy-MM-ddT00:00:00Z")
        $schedBody = @{
            companyId = $companyId
            assetId = $assetId
            frequency = 1  # Monthly
            description = "Monthly preventive maintenance and calibration check for CNC precision milling machine."
            nextDueDate = $nextDueDate
        } | ConvertTo-Json

        $schedRes = Invoke-RestMethod -Uri "$BaseUrl/maintenance/schedules" -Method Post -Headers $headers -ContentType "application/json" -Body $schedBody
        Write-Host "  [OK] Maintenance Schedule created: Monthly check for CNC-MILL-01, next due $nextDueDate" -ForegroundColor Green
    } catch {
        Write-Host "  [INFO] Maintenance Schedule creation: $_" -ForegroundColor Gray
    }
} else {
    Write-Host "  [INFO] Maintenance Schedule skipped - CNC-MILL-01 asset not available." -ForegroundColor Gray
}

Write-Host "FusionOS Enterprise Tenant Seeding Completed Successfully!" -ForegroundColor Green
