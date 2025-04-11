using ClosedXML.Excel;
using EzCondo_Data.Context;
using EzConDo_Service.DTO;
using EzConDo_Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Implement
{
    public class ElectricMetterService : IElectricMeterService
    {
        private readonly ApartmentDbContext dbContext;

        public ElectricMetterService(ApartmentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<ElectricMetterDTO>> AddElectricMetters(IFormFile file)
        {
            if(file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            var electricMetters = new List<ElectricMetterDTO>();

            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1);
                foreach (var row in rows)
                {
                    //id
                    string idValue = row.Cell(1).GetValue<string>().Trim();
                    Guid? id = null;
                    if (Guid.TryParse(idValue, out Guid guidValue))
                    {
                        id = guidValue;
                    }

                    //MeterNumber
                    string meterNumber = row.Cell(2).GetValue<string>().Trim();

                    //InstallationDate
                    DateOnly installationDate;
                    var cell = row.Cell(3);

                    if (cell.DataType == XLDataType.DateTime)
                    {
                        installationDate = DateOnly.FromDateTime(cell.GetDateTime());
                    }
                    else if (cell.DataType == XLDataType.Number)
                    {
                        installationDate = DateOnly.FromDateTime(DateTime.FromOADate(cell.GetDouble()));
                    }
                    else
                    {
                        string installationDateValue = cell.GetValue<string>().Trim();
                        var validFormats = new[] { "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy", "d/M/yyyy", "dd/MM/yyyy" };

                        if (!DateOnly.TryParseExact(installationDateValue, validFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out installationDate))
                        {
                            throw new Exception($"Invalid installation date at the line: {row.RowNumber()} - Value: {installationDateValue}");
                        }
                    }

                    //ApartmentNumber
                    string apartmentNumber = row.Cell(4).GetValue<string>().Trim();
                    var apartment = await dbContext.Apartments.FirstOrDefaultAsync(a => a.ApartmentNumber == apartmentNumber) 
                        ?? throw new Exception($"'{apartmentNumber}' at row: {row.RowNumber()} is not found"); ;


                    var dto = new ElectricMetterDTO
                    {
                        Id = id,
                        MeterNumber = meterNumber,
                        InstallationDate = installationDate,
                        ApartmentId = apartment.Id
                    };

                    electricMetters.Add(dto);
                }
                //map dto -> domain
                var entities = electricMetters.Select(dto => new ElectricMeter
                {
                    Id = Guid.NewGuid(),
                    MeterNumber = dto.MeterNumber,
                    InstallationDate = dto.InstallationDate,
                    ApartmentId = dto.ApartmentId
                });

                await dbContext.AddRangeAsync(entities);
                await dbContext.SaveChangesAsync();

                return electricMetters;
            }
        }
    }
}
