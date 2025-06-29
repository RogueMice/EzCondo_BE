﻿using EzCondo_Data.Context;
using EzCondo_Data.Domain;
using EzConDo_Service.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzConDo_Service.Interface
{
    public interface ICitizenService
    {
        Task<Citizen> AddOrUpdateCitizenAsync(CitizenDTO citizenDTO);

        Task<List<CitizenViewDTO>> GetAllCitizensAsync();
    }
}
