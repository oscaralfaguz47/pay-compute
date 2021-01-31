using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayCompute.Entity;
using PayCompute.Services;
using PayCompute.UI.Models.PaymentRecords;
using RotativaCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayCompute.UI.Controllers
{
    [Authorize(Roles ="Admin, Manager")] // The Admin and Manager users have permision
    public class PayController : Controller
    {
        private readonly IPayComputationService _payComputationService;
        private readonly IEmployeeService _employeeService;
        private readonly ITaxService _taxService;
        private readonly INationalInsuranceContributionService _nationalInsuranceContributionService;
        private decimal overtimeHrs;
        private decimal contractualEarnings;
        private decimal overtimeEarnings;
        private decimal totalEarnings;
        private decimal tax;
        private decimal unionFee;
        private decimal studentLoan;
        private decimal nationalInsurance;
        private decimal totalDeduction;

        public PayController(
            IPayComputationService payComputationService, 
            IEmployeeService employeeService, 
            ITaxService taxService,
            INationalInsuranceContributionService nationalInsuranceContributionService)
        {
            _payComputationService = payComputationService;
            _employeeService = employeeService;
            _taxService = taxService;
            _nationalInsuranceContributionService = nationalInsuranceContributionService;
        }

        
        public IActionResult Index()
        {
            var payRecords = _payComputationService.GetAll().Select(pay => new PaymentRecordIndexViewModel
            {
                Id = pay.Id,
                EmployeeId = pay.EmployeeId,
                FullName = pay.FullName,
                PayDate = pay.PayDate,
                PayMonth = pay.PayMonth,
                TaxYearId = pay.TaxYearId,
                Year =  _payComputationService.GetTaxYearById(pay.TaxYearId).YearOfTax,
                TotalEarnings = pay.TotalEarnings,
                TotalDeduction = pay.TotalDeduction,
                NetPayment = pay.NetPayment,
                Employee = pay.Employee
            });
            return View(payRecords);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.employees = _employeeService.GetAllEmployeesForPayroll(); 
            ViewBag.taxYears = _payComputationService.GetAllTaxYear();
            var model = new PaymentRecordCreateViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Create(PaymentRecordCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var payrecord = new PaymentRecord()
                {
                    Id = model.Id,
                    EmployeeId = model.EmployeeId,
                    FullName = _employeeService.GetById(model.EmployeeId).FullName,
                    NationalNumber = _employeeService.GetById(model.EmployeeId).NationalInsuranceNo,
                    PayDate = model.PayDate,
                    PayMonth = model.PayMonth,
                    TaxYearId = model.TaxYearId,
                    TaxCode = model.TaxCode,
                    HourlyRate = model.HourlyRate,
                    HoursWorked = model.HoursWorked,
                    ContractualHours = model.ContractualHours,
                    OvertimeHours = overtimeHrs = _payComputationService.OvertimeHours(model.HoursWorked, model.ContractualHours),
                    ContractualEarnings = contractualEarnings = _payComputationService.ContractualEarnings(model.ContractualHours, model.HoursWorked, model.HourlyRate),
                    OvertimeEarnings = overtimeEarnings = _payComputationService.OvertimeEarnings(_payComputationService.OvertimeRate(model.HourlyRate), overtimeHrs),
                    TotalEarnings = totalEarnings = _payComputationService.TotalEarnings(overtimeEarnings, contractualEarnings),
                    Tax = tax = _taxService.TaxAmount(totalEarnings),
                    UnionFee = unionFee = _employeeService.UnionFees(model.EmployeeId),
                    SLC = studentLoan = _employeeService.StudentLoanRepaymentAmount(model.EmployeeId, totalEarnings),
                    NationalContribution = nationalInsurance = _nationalInsuranceContributionService.NIContribution(totalEarnings),
                    TotalDeduction = totalDeduction = _payComputationService.TotalDeduction(tax, nationalInsurance, studentLoan, unionFee),
                    NetPayment = _payComputationService.NetPay(totalEarnings, totalDeduction)
                };
                await _payComputationService.CreateAsync(payrecord);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.employees = _employeeService.GetAllEmployeesForPayroll();
            ViewBag.taxYears = _payComputationService.GetAllTaxYear();
            return View();
        }

        public IActionResult Detail(int id)
        {
            var paymetRecord = _payComputationService.GetById(id);
            if(paymetRecord == null){
                return NotFound();
            }
            var model = new PaymentRecordDetailViewModel()
            {
                Id = paymetRecord.Id,
                EmployeeId = paymetRecord.EmployeeId,
                FullName = paymetRecord.FullName,
                NationalNumber = paymetRecord.NationalNumber,
                PayDate = paymetRecord.PayDate,
                PayMonth = paymetRecord.PayMonth,
                TaxYearId = paymetRecord.TaxYearId,
                Year = _payComputationService.GetTaxYearById(paymetRecord.TaxYearId).YearOfTax,
                TaxCode = paymetRecord.TaxCode,
                HourlyRate = paymetRecord.HourlyRate,
                HoursWorked = paymetRecord.HoursWorked,
                ContractualHours = paymetRecord.ContractualHours,
                OvertimeHours = paymetRecord.OvertimeHours,
                OvertimeRate = _payComputationService.OvertimeRate(paymetRecord.HourlyRate),
                ContractualEarnings = paymetRecord.ContractualEarnings,
                OvertimeEarnings = paymetRecord.OvertimeEarnings,
                Tax = paymetRecord.Tax,
                NationalContribution = paymetRecord.NationalContribution,
                UnionFee = paymetRecord.UnionFee,
                SLC = paymetRecord.SLC,
                TotalEarnings = paymetRecord.TotalEarnings,
                TotalDeduction = paymetRecord.TotalDeduction,
                Employee = paymetRecord.Employee,
                TaxYear = paymetRecord.TaxYear,
                NetPayment = paymetRecord.NetPayment
            };
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Payslip(int id)
        {
            var paymetRecord = _payComputationService.GetById(id);
            if (paymetRecord == null)
            {
                return NotFound();
            }
            var model = new PaymentRecordDetailViewModel()
            {
                Id = paymetRecord.Id,
                EmployeeId = paymetRecord.EmployeeId,
                FullName = paymetRecord.FullName,
                NationalNumber = paymetRecord.NationalNumber,
                PayDate = paymetRecord.PayDate,
                PayMonth = paymetRecord.PayMonth,
                TaxYearId = paymetRecord.TaxYearId,
                Year = _payComputationService.GetTaxYearById(paymetRecord.TaxYearId).YearOfTax,
                TaxCode = paymetRecord.TaxCode,
                HourlyRate = paymetRecord.HourlyRate,
                HoursWorked = paymetRecord.HoursWorked,
                ContractualHours = paymetRecord.ContractualHours,
                OvertimeHours = paymetRecord.OvertimeHours,
                OvertimeRate = _payComputationService.OvertimeRate(paymetRecord.HourlyRate),
                ContractualEarnings = paymetRecord.ContractualEarnings,
                OvertimeEarnings = paymetRecord.OvertimeEarnings,
                Tax = paymetRecord.Tax,
                NationalContribution = paymetRecord.NationalContribution,
                UnionFee = paymetRecord.UnionFee,
                SLC = paymetRecord.SLC,
                TotalEarnings = paymetRecord.TotalEarnings,
                TotalDeduction = paymetRecord.TotalDeduction,
                Employee = paymetRecord.Employee,
                TaxYear = paymetRecord.TaxYear,
                NetPayment = paymetRecord.NetPayment
            };
            return View(model);
        }

        public IActionResult GeneratePayslipPdf(int id)
        {
            var payslip = new ActionAsPdf("Payslip", new { id = id })
            {
                FileName = "payslip.pdf"
            };
            return payslip;  
        }
    }
}
