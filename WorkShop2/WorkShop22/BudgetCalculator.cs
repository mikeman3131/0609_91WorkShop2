﻿using System;
using System.Collections.Generic;
using System.Linq;
using WorkShop2.Tests;

namespace WorkShop22
{
    public class BudgetCalculator
    {
        private readonly IRepository<Budget> _budRepository;

        public BudgetCalculator(IRepository<Budget> budRepository)
        {
            _budRepository = budRepository;
        }

        internal decimal TotalAmount(DateTime startTime, DateTime endTime)
        {
            var period = new Period(startTime, endTime);
            var budgets = _budRepository.GetBudgets();
            var budget = budgets.SingleOrDefault(x => x.YearMonth == period.StartTime.ToString("yyyyMM"));

            if (period.IsSameMonth())
            {
                if (budget == null)
                {
                    return 0;
                }

                return period.Days() * budget.DailyAmount();
            }

            var total = TotalAmountWhenPeriodOverlapMultiMonths(period, budgets);
            return total;
        }

        private static decimal TotalAmountWhenPeriodOverlapMultiMonths(Period period, List<Budget> budgets)
        {
            var total = 0m;
            var currentMonth = period.StartTime;
            while (currentMonth <= period.EndTime.AddMonths(1))
            {
                var overlapStartDate = currentMonth.ToString("yyyyMM") == period.StartTime.ToString("yyyyMM")
                    ? period.StartTime
                    : GetFirstDay(currentMonth);

                var overlapEndDate = currentMonth.ToString("yyyyMM") == period.EndTime.ToString("yyyyMM")
                    ? period.EndTime
                    : GetLastDay(currentMonth);

                var amountOfCurrentMonth = CalculateBudget(overlapStartDate, overlapEndDate, budgets);
                total += amountOfCurrentMonth;
                currentMonth = currentMonth.AddMonths(1);
            }

            return total;
        }

        private static DateTime GetFirstDay(DateTime currentDate)
        {
            return new DateTime(currentDate.Year, currentDate.Month, 1);
        }

        private static DateTime GetLastDay(DateTime currentDate)
        {
            return new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1).AddDays(-1);
        }

        private static bool IsOver2Months(DateTime startTime, DateTime endTime)
        {
            var startTime1 = new DateTime(startTime.Year, startTime.Month, 1);
            var endTime1 = new DateTime(endTime.Year, endTime.Month, 1);
            return startTime1.AddMonths(2) < endTime1;
        }

        private static int CalculateBudget(DateTime startTime, DateTime endTime, List<Budget> budgets)
        {
            var budget = budgets.SingleOrDefault(x => x.YearMonth == startTime.ToString("yyyyMM"));
            if (budget == null)
            {
                return 0;
            }

            var days = endTime.Subtract(startTime).Days + 1;
            return days * budget.DailyAmount();
        }
    }
}