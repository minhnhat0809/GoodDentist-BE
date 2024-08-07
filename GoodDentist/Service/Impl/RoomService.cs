﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.DTO;
using BusinessObject.DTO.RoomDTOs;
using BusinessObject.Entity;
using Microsoft.IdentityModel.Tokens;
using Repositories;

namespace Services.Impl;

public class RoomService : IRoomService
{
	private readonly IMapper mapper;
	private readonly IUnitOfWork unitOfWork;

	public RoomService(IMapper mapper, IUnitOfWork unitOfWork)
	{
		this.mapper = mapper;
		this.unitOfWork = unitOfWork;
	}

	public ResponseDTO ValidateRoom(CreateRoomDTO roomDTO)
	{
		if (roomDTO.RoomNumber.IsNullOrEmpty())
		{
			return new ResponseDTO("RoomNumber cannot be emty", 400, false, null);
		}
		String pattern = "^\\d+$";
		var regex = new Regex(pattern);
		if (!regex.IsMatch(roomDTO.RoomNumber.ToString()))
		{
			return new ResponseDTO("RoomNumber cannot contains any letter", 400, false, null);
		}
		if (checkDuplicate(roomDTO))
		{
			return new ResponseDTO("RoomNumber cannot be duplicated", 400, false, null);
		}
		return new ResponseDTO("ok", 200, true, null);

	}

	private bool checkDuplicate(CreateRoomDTO roomDTO)
	{
		return unitOfWork.roomRepo.CheckDuplicateRoom(roomDTO);
	}

	public async Task<ResponseDTO> getAllRoom(int pageNumber, int rowsPerPage)
	{
		try
		{
			List<Room> roomList = await unitOfWork.roomRepo.GetAllRoom(pageNumber, rowsPerPage); //
			List<Room> rooms = mapper.Map<List<Room>>(roomList);
			return new ResponseDTO("Get rooms successfully!", 200, true, null);
		}
		catch (Exception ex)
		{
			return new ResponseDTO(ex.Message, 500, false, null);
		}
	}

	public async Task<ResponseDTO> deleteRoom(int roomId)
	{
		Room room = await unitOfWork.roomRepo.GetRoomByID(roomId);
		if (room == null)
		{
			return new ResponseDTO("Cannot find the room", 400, false, null);
		}
		try
		{
			CreateRoomDTO createRoomDTO = new CreateRoomDTO();
			createRoomDTO.RoomId = room.RoomId;
			createRoomDTO.RoomNumber = room.RoomNumber;
			createRoomDTO.ClinicId = room.ClinicId;
			createRoomDTO.Status = false;
			unitOfWork.roomRepo.Detach(room);
			Room r = mapper.Map<Room>(createRoomDTO);
			unitOfWork.roomRepo.Attach(r);
			var update = await unitOfWork.roomRepo.UpdateAsync(r);
			if (!update)
			{
				return new ResponseDTO("Failed to update", 500, false, null);

			}
			return new ResponseDTO("Sucessfully", 200, true, null);
		}
		catch (Exception ex)
		{
			return new ResponseDTO(ex.Message, 500, false, null);
		}
	}

	public async Task<ResponseDTO> createRoom(CreateRoomDTO model)
	{
		var check = ValidateRoom(model);
		if (check.IsSuccess)
		{

			try
			{
				Room room = mapper.Map<Room>(model);
				var create = await unitOfWork.roomRepo.CreateAsync(room);
				if (create)
				{
					return new ResponseDTO("Sucessfully", 200, true, null);
				}
				else
				{
					return new ResponseDTO("Unsucessfully", 500, false, null);
				}

			}
			catch (Exception ex)
			{
				return new ResponseDTO(ex.Message, 500, false, null);
			}
		}
		return new ResponseDTO(check.Message, 400, true, null);
	}

	public async Task<ResponseDTO> updateRoom(CreateRoomDTO model)
	{
		Room room = await unitOfWork.roomRepo.GetRoomByID(model.RoomId);
		if (room == null)
		{
			return new ResponseDTO("Cannot find the room", 400, false, null);
		}
		var check = ValidateRoom(model);
		if (check.IsSuccess)
		{
			try
			{
				int roomId = model.RoomId;
				unitOfWork.roomRepo.Detach(room);
				CreateRoomDTO createRoomDTO = new CreateRoomDTO();
				createRoomDTO.RoomNumber = model.RoomNumber;
				createRoomDTO.ClinicId = model.ClinicId; ;
				Room r = mapper.Map<Room>(createRoomDTO);
				r.RoomId = roomId;
				r.Status = true;
				unitOfWork.roomRepo.Attach(r);
				var update = await unitOfWork.roomRepo.UpdateAsync(r);
				if (!update)
				{
					return new ResponseDTO("Failed to update", 500, false, null);

				}
				return new ResponseDTO("Sucessfully", 200, true, null);
			}
			catch (Exception ex)
			{
				return new ResponseDTO(ex.Message, 500, false, null);
			}
		}
		return new ResponseDTO(check.Message, 500, false, null);
	}
}
