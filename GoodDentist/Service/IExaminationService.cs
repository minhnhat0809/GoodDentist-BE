﻿using BusinessObject.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.DTO;
using Microsoft.AspNetCore.Mvc;
using BusinessObject.DTO.ExaminationDTOs;

namespace Services
{
    public interface IExaminationService
    {
        Task<ResponseDTO> GetExaminationById(int examId);

        Task<ResponseListDTO> CreateExamination(ExaminationRequestDTO examinationDTO, string mod, string mode, string? customerId);

        Task<ResponseListDTO> UpdateExamination(ExaminationRequestDTO examinationDTO, string mod);

        Task<ResponseDTO> DeleteExamination(int examId);

        Task<ResponseDTO> GetAllExaminationOfUser(string clinicId, string userId, string actor, DateOnly selectedDate, int pageNumber, int rowsPerPage,
            string? sortField = null,
            string? sortOrder = "asc");

        Task<ResponseDTO> GetAllExaminationOfClinic(string clinicId, int pageNumber, int rowsPerPage,
            string? sortField = null,
            string? sortOrder = "asc");

        Task<ResponseDTO> GetAllExaminationOfExaminationProfile(int examProfileId, int pageNumber, int rowsPerPage,
           string? sortField = null,
           string? sortOrder = "asc");
    }
}
