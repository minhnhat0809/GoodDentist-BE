﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObject.DTO;
using BusinessObject.DTO.OrderDTOs;
using BusinessObject.DTO.OrderDTOs.View;
using BusinessObject.DTO.ServiceDTOs.View;
using BusinessObject.Entity;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Impl;
using static StackExchange.Redis.Role;

namespace Services.Impl
{
    public class OrderServices : IOrderServices
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
        public OrderServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
			_mapper = mapper;
        }
		public async Task<ResponseDTO> GetAllOrder(int pageNumber, int pageSize)
		{
			try
			{
				List<Order>? orderList = await _unitOfWork.orderRepo.GetAllOrder(pageNumber, pageSize);
				var all = orderList.Where(c => c.Status == true);

				List<OrderDTO> orderDTOList = _mapper.Map<List<OrderDTO>>(all);
				return new ResponseDTO("Get all Order successfully!", 200, true,orderDTOList);
			}
			catch (Exception ex)
			{
				return new ResponseDTO(ex.Message, 500, false, null);
			}
		}

		public async Task<ResponseDTO> SearchOrder(string searchValue)
		{
			try
			{
				List<Order>? orderList = await _unitOfWork.orderRepo.FindByConditionAsync(c => c.OrderName == searchValue);
				var all = orderList.Where(c => c.Status == true);
				List<OrderDTO> orderDTOList = _mapper.Map<List<OrderDTO>>(all);
				if (orderDTOList.IsNullOrEmpty())
				{
					return new ResponseDTO("No result found!", 200, true, null);
				}

				return new ResponseDTO("Search Order successfully!", 200, true, orderDTOList);
			}
			catch (Exception ex)
			{
				return new ResponseDTO(ex.Message, 500, false, null);
			}
		}

		public async Task<ResponseDTO> DeleteOrder(int orderId)
		{
			try
			{
				var order = await _unitOfWork.orderRepo.GetOrderById(orderId);
				if (order != null)
				{
					List<OrderService> orderServices = order.OrderServices.ToList();
					foreach (var orderService in orderServices)
					{
						if (orderService.Status != 1 ) return new ResponseDTO("This Order is on processing! Can Not Delete !", 400, false, orderService);
					}
					order = await _unitOfWork.orderRepo.DeleteOrder(orderId);
					return new ResponseDTO("Order Delete successfully!", 200, true, _mapper.Map<OrderDTO>(order));

				}
				return new ResponseDTO("This order is not exist!", 400, false, null);
			}
			catch (Exception ex)
			{
				return new ResponseDTO(ex.Message, 500, false, null);
			}
		}

        public async Task<ResponseDTO> GetOrderDetails(int orderId)
        {
            ResponseDTO responseDto = new ResponseDTO("", 200, true, null);
            try
            {
                if (orderId <= 0)
                {
                    responseDto.IsSuccess = false;
                    responseDto.StatusCode = 400;
                    responseDto.Message = "Order Id is null!";
                    return responseDto;
                }

                Order? order = await _unitOfWork.orderRepo.GetOrderById(orderId);
                if (order == null)
                {
                    responseDto.IsSuccess = false;
                    responseDto.StatusCode = 404;
                    responseDto.Message = "Order is not found!";
                    return responseDto;
                }

                OrderDTO orderDto = _mapper.Map<OrderDTO>(order);

                responseDto.Result = orderDto;
                responseDto.Message = "Get successfully!";
            }
            catch (Exception e)
            {
                responseDto.IsSuccess = false;
                responseDto.StatusCode = 500;
                responseDto.Message = e.Message;
            }

            return responseDto;
        }

        public async Task<ResponseDTO> UpdateOrderAfterPayment(Order order)
        {
	        try
	        {
		        if (order!=null)
		        {
			        Order? model = await _unitOfWork.orderRepo.GetOrderById(order.OrderId);
			        if (model != null)
			        {
				        foreach (OrderService? orderService in order.OrderServices.ToList())
				        {
					        orderService.Status = 5;
				        }
				        model = await _unitOfWork.orderRepo.UpdateOrder(order);
				        return new ResponseDTO("Update Order Price successfully", 200, true,
					        _mapper.Map<OrderDTO>(order));

			        } return new ResponseDTO("Order not exist !", 404, false, _mapper.Map<OrderDTO>(order));
		        } return new ResponseDTO("Order is should not null!", 404, false, _mapper.Map<OrderDTO>(order));
            }
            catch (Exception ex)
            {
                return new ResponseDTO(ex.Message, 500, false, null);
            }
        }

        public async Task<ResponseDTO> AddOrder(OrderCreateDTO orderDTO)
		{
			try
			{
				/*var check = await CheckValidationAddOrder(orderDTO);
				if (check.IsSuccess == false)
				{
					return check;
				}*/
				
				// map to model
				Order model = _mapper.Map<Order>(orderDTO);
				model.Price = 0;
				model.DateTime = DateTime.Now;
				model.Status = true;
				if(!orderDTO.Services.IsNullOrEmpty())
				{
					foreach (ServiceToOrderDTO serviceDto in orderDTO.Services)
					{
						var service = await _unitOfWork.serviceRepo.GetByIdAsync(serviceDto.ServiceId);
						if (service!=null)
						{
							OrderService orderService = new OrderService
							{
								Price = service.Price,
								Quantity = serviceDto.Quantity,
								Status = 1,
								ServiceId = service.ServiceId
							};
							model.OrderServices.Add(orderService);
							model.Price += service.Price * serviceDto.Quantity;
						}
					}
				}

				if (orderDTO.ExaminationId != null) 
				{
					Examination? examination =
						await _unitOfWork.examinationRepo.GetExaminationById(orderDTO.ExaminationId.Value);
					if (examination == null) return new ResponseDTO("Examination not found!", 400, false, examination);
					model.Examination = examination;
				}
				
				model = await _unitOfWork.orderRepo.CreateOrder(model);
				return new ResponseDTO("Create successfully", 200, true, _mapper.Map<OrderDTO>(model));
			}
			catch (Exception ex)
			{
				return new ResponseDTO(ex.Message, 500, false, null);
			}
		}

		public async Task<ResponseDTO> UpdateOrder(OrderUpdateDTO orderDTO)
		{
			try
			{
				Order model = await _unitOfWork.orderRepo.GetOrderById(orderDTO.OrderId);
				
				if (model == null)
				{
					return new ResponseDTO("This order is not exist!", 400, false, null);
				}/*
				var check = await CheckValidationUpdateOrder(orderDTO);
				if (check.IsSuccess == false)
				{
					return check;
				}*/
				// map to model
				model = _mapper.Map<Order>(orderDTO);
				model.Price = 0;
				if(!orderDTO.Services.IsNullOrEmpty())
				{
					foreach (ServiceToOrderDTO serviceDto in orderDTO.Services)
					{
						var service = await _unitOfWork.serviceRepo.GetByIdAsync(serviceDto.ServiceId);
						if (service!=null)
						{
							OrderService orderService = new OrderService
							{
								Order = model,
								OrderId = model.OrderId,
								Price = service.Price,
								Quantity = serviceDto.Quantity,
								Status = serviceDto.Status,
								ServiceId = service.ServiceId,
								Service = service
							};
							model.OrderServices.Add(orderService);
							if(orderService.Status != 0 ) model.Price += service.Price * serviceDto.Quantity;
						}
					}
				}

				if (orderDTO.ExaminationId != null) 
				{
					model.Examination =
						await _unitOfWork.examinationRepo.GetExaminationById(orderDTO.ExaminationId.Value);
				}
				await _unitOfWork.orderRepo.UpdateOrder(model);
				return new ResponseDTO("Update Successfully!", 200, true, _mapper.Map<OrderDTO>(model));
			}
			catch (Exception ex)
			{
				return new ResponseDTO(ex.Message, 500, false, null);
			}
		}

		public async Task<ResponseDTO> CheckValidationAddOrder(OrderCreateDTO orderDTO)
		{
			if (orderDTO.OrderName.IsNullOrEmpty())
			{
				return new ResponseDTO("Please input order name", 400, false, null);
			}

			/*if(orderDTO.ExaminationId.ToString().IsNullOrEmpty())
			{
				return new ResponseDTO("Please input examinationID", 400, false, null);
			}*/

			if (orderDTO.DateTime.ToString().IsNullOrEmpty())
			{
				return new ResponseDTO("Please choose Date Time!", 400, false, null);
			}

			if(orderDTO.Price.ToString().IsNullOrEmpty())
			{
				return new ResponseDTO("Please input order's price!", 400, false, null);
			}

			List<Order> order = await _unitOfWork.orderRepo.FindByConditionAsync(c => c.Status == true);
			if (order.Any(c => c.OrderName == orderDTO.OrderName))
			{
				return new ResponseDTO("Order name is already existed!", 400, false, null);
			}

			if(orderDTO.Price < 0)
			{
				return new ResponseDTO("Order's price must be greater than 0!", 400, false, null);
			}

			if(orderDTO.Price == null)
			{
				return new ResponseDTO("Please input order's price", 400, false, null);
			}

			if(orderDTO.Price > decimal.MaxValue)
			{
				return new ResponseDTO("Order's price is out of range!", 400, false, null);
			}

			return new ResponseDTO("Check validation successfully", 200, true, null);
		}

		public async Task<ResponseDTO> CheckValidationUpdateOrder(OrderDTO orderDTO)
		{
			if(orderDTO.OrderId.ToString().IsNullOrEmpty())
			{
				return new ResponseDTO("Please choose order!", 400, false, null);
			}
			if (orderDTO.OrderName.IsNullOrEmpty())
			{
				return new ResponseDTO("Please input order name", 400, false, null);
			}

			/*if (orderDTO.ExaminationId.ToString().IsNullOrEmpty())
			{
				return new ResponseDTO("Please input examinationID", 400, false, null);
			}*/

			if (orderDTO.DateTime.ToString().IsNullOrEmpty())
			{
				return new ResponseDTO("Please choose Date Time!", 400, false, null);
			}

			if (orderDTO.Price.ToString().IsNullOrEmpty())
			{
				return new ResponseDTO("Please input order's price!", 400, false, null);
			}

			List<Order> orders = await _unitOfWork.orderRepo.FindByConditionAsync(c => c.Status == true);
			if (orders.Any(c => c.OrderName == orderDTO.OrderName && c.OrderId != orderDTO.OrderId))
			{
				return new ResponseDTO("Order name is already existed!", 400, false, null);
			}

			if (orderDTO.Price < 0)
			{
				return new ResponseDTO("Order's price must be greater than 0!", 400, false, null);
			}

			if (orderDTO.Price == null)
			{
				return new ResponseDTO("Please input order's price", 400, false, null);
			}

			if (orderDTO.Price > decimal.MaxValue)
			{
				return new ResponseDTO("Order's price is out of range!", 400, false, null);
			}
			return new ResponseDTO("Check validation successfully", 200, true, null);
		}

	}
}
