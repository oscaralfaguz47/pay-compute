using Microsoft.AspNetCore.Mvc;
using PayCompute.Services;
using PayCompute.UI.Models.PaymentRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayCompute.UI.Controllers
{
    public class PayController : Controller
    {
        private readonly IPayComputationService _payComputationService;
        public PayController(IPayComputationService payComputationService)
        {
            _payComputationService = payComputationService;
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

        public IActionResult Create()
        {

            return View();
        }
    }
}
