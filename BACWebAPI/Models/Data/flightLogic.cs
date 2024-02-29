using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using BACWebAPI.Models.Service;
using BusinessClassLibrary.EntityClass;
using BusinessClassLibrary.LogicClass.Admin;
using BusinessClassLibrary.LogicClass.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BACWebAPI.Models.Data
{
	public class flightLogic
	{
		private readonly APIRequestLOG objAPIRequestLOG = new APIRequestLOG();
		private readonly clsAPILOG objclsAPILOG = new clsAPILOG();
		private readonly clsCommonFunction objCommonFunction = new clsCommonFunction();
		private readonly clsFlightDetail objFlightDetail = new clsFlightDetail();
		private readonly clsFlightDTO objFlightDTO = new clsFlightDTO();

		public List<FlightDetail> getArrivalFligths_old()
		{
			string strFlightArrivalResponse = "", strFlightDepartureResponse = "", strCurrentTime = "";
			DateTime dtCurrentDate;

			var newAdditionalSpan = new TimeSpan(2, 0, 0);
			var newSubtractSpan = new TimeSpan(2, 0, 0);

			objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);
			var tspnCurrentTime = TimeSpan.Parse(strCurrentTime);

			strFlightArrivalResponse =
				objCommonFunction.SendRequest(KeyValues.FlightArrivalServiceURL + KeyValues.FlightAPIKey, "");
			var objParseArrivalResult = JObject.Parse(Convert.ToString(strFlightArrivalResponse));
			var lstFlightArrival =
				JsonConvert.DeserializeObject<List<FlightDetail>>(Convert.ToString(objParseArrivalResult["fidsData"]));

			var lstLessTimeFlightArrival = new List<FlightDetail>(lstFlightArrival);
			var lstMoreTimeFlightArrival = new List<FlightDetail>(lstFlightArrival);
			var lstFlightArrivalFliterList = new List<FlightDetail>();

			lstLessTimeFlightArrival = lstFlightArrival.Where(x =>
					TimeSpan.Parse(x.currentTime) > tspnCurrentTime.Subtract(newSubtractSpan) &&
					TimeSpan.Parse(x.currentTime) <= tspnCurrentTime)
				.OrderByDescending(r => r.currentTime)
				.Take(4).ToList();

			lstMoreTimeFlightArrival = lstFlightArrival.Where(t =>
					TimeSpan.Parse(t.currentTime) >= tspnCurrentTime &&
					TimeSpan.Parse(t.currentTime) <= tspnCurrentTime.Add(newAdditionalSpan))
				.OrderBy(p => p.currentTime)
				.Take(4).ToList();

			lstFlightArrivalFliterList.AddRange(lstLessTimeFlightArrival);
			lstFlightArrivalFliterList.AddRange(lstMoreTimeFlightArrival);


			#region DEPARTURE

			//strDepartureAPIUrl = strDepartureAPIUrl?.Replace("#APPKEY#", strAppKey);

			strFlightDepartureResponse =
				objCommonFunction.SendRequest(KeyValues.FlightArrivalServiceURL + KeyValues.FlightAPIKey, "");
			var objParseDepartureResult = JObject.Parse(Convert.ToString(strFlightDepartureResponse));
			var lstFlightDeparture =
				JsonConvert.DeserializeObject<List<FlightDetail>>(
					Convert.ToString(objParseDepartureResult["fidsData"]));

			var lstLessTimeFlightDeparture = new List<FlightDetail>(lstFlightDeparture);
			var lstMoreTimeFlightDeparture = new List<FlightDetail>(lstFlightDeparture);
			var lstFlightDepartureFliterList = new List<FlightDetail>();

			lstLessTimeFlightDeparture = lstFlightDeparture
				.Where(x => TimeSpan.Parse(x.currentTime) > tspnCurrentTime.Subtract(newSubtractSpan) &&
							TimeSpan.Parse(x.currentTime) <=
							tspnCurrentTime) //TimeSpan.Compare(TimeSpan.Parse(x.currentTime), tspnCurrentTime.Subtract(newSubtractSpan)) == -1
				.OrderByDescending(r => r.currentTime)
				.Take(4).ToList();

			lstMoreTimeFlightDeparture = lstFlightDeparture.Where(t =>
					TimeSpan.Parse(t.currentTime) >= tspnCurrentTime &&
					TimeSpan.Parse(t.currentTime) <= tspnCurrentTime.Add(newAdditionalSpan))
				.OrderBy(p => p.currentTime)
				.Take(4).ToList();

			lstFlightDepartureFliterList.AddRange(lstLessTimeFlightDeparture);
			lstFlightDepartureFliterList.AddRange(lstMoreTimeFlightDeparture);

			#endregion

			return lstFlightArrivalFliterList;
		}

		public void GetArrivalFligths(FlightRequest flightRequest, ref CommonResponse<List<FlightDetail>> objResponse,
			ModelStateDictionary modalState)
		{
			var objError = new List<clsMessage>();
			if (modalState.IsValid)
			{
				try
				{
					var strCurrentTime = "";
					DateTime dtCurrentDate;

					var newDisplayTimeSpan = new TimeSpan(0, 20, 0);
					var newDisplaySubsractTimeSpan = new TimeSpan(6, 0, 0);
					var newDisplayAdditionTimeSpan = new TimeSpan(12, 0, 0);

					objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);
					var tspnCurrentTime = TimeSpan.Parse(strCurrentTime);

					var lstFlightArrivalFliterList = new List<FlightDetail>();
					var lstFlightArrival = BindArrivalFlights();

					if (flightRequest.ReqType == RequestFromType.Dashboard && flightRequest.dateType == "today")
					{
						// SELECT DATA FROM -6 AND +12
						/*lstFlightArrival = lstFlightArrival
												.Where(
												(x =>
													(TimeSpan.Parse(x.currentTime) >= tspnCurrentTime.Subtract(newDisplaySubsractTimeSpan) && TimeSpan.Parse(x.currentTime) <= tspnCurrentTime)
													|| (TimeSpan.Parse(x.currentTime) >= tspnCurrentTime && TimeSpan.Parse(x.currentTime) <= tspnCurrentTime.Add(newDisplayAdditionTimeSpan))
													)
												)
											.ToList();*/

						//ADD ALL FLIGHTS WHERE HAVE NOT ANY REMARKS
						/*lstFlightArrivalFliterList.AddRange(lstFlightArrival.Where(a => a.remarks == "" || a.remarks == null).ToList());*/

						//ADD ALL DELAYED FLIGHTS
						/*lstFlightArrivalFliterList.AddRange(lstFlightArrival.Where(a => a.remarks == FlightRemarks.Delayed).ToList());*/

						//ADD OTHER FLIGHTS 20 MIN
						/*lstFlightArrivalFliterList.AddRange(lstFlightArrival
													.Where(a => (a.remarks != FlightRemarks.Delayed)
														&& (TimeSpan.Parse(a.currentTime) > tspnCurrentTime.Subtract(newDisplayTimeSpan) || TimeSpan.Parse(a.currentTime) < tspnCurrentTime.Add(newDisplayTimeSpan))
														)
														.ToList()
													);*/


						lstFlightArrival = (from item in lstFlightArrival
								select
									new FlightDetail
									{
										//currentDateTime = Convert.ToDateTime(item.currentDate + " " + item.currentTime),
										currentDateTime = Convert.ToDateTime(
											objCommonFunction.ConvertDateTimeFormat(item.currentDate, item.currentTime,
												"")),
										currentDate = item.currentDate,
										currentTime = item.currentTime,
										airlineCode = item.airlineCode,
										scheduledTime = item.scheduledTime,
										scheduledDate = item.scheduledDate,
										terminal = item.terminal,
										originCity = item.originCity,
										airlineName = item.airlineName,
										airportCode = item.airportCode,
										delayed = item.delayed,
										destinationAirportCode = item.destinationAirportCode,
										destinationCity = item.destinationCity,
										flight = item.flight,
										flightNumber = item.flightNumber,
										originAirportCode = item.originAirportCode,
										remarks = item.remarks,
										remarksWithTime = item.remarksWithTime,
										checkinCounter = item.checkinCounter,
                                        baggage = item.baggage,
                                        gate = item.gate,
										actualTime = item.actualTime,
										actualDate = item.actualDate
									})
							.ToList();

						/*var before = lstFlightArrival.Where(t => TimeSpan.Parse(t.currentTime) <= tspnCurrentTime)
																.OrderByDescending(p => p.currentTime)
																.Take(Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(flightRequest.count / 2)))).ToList();

						lstFlightArrivalFliterList.AddRange(before.OrderBy(x=>x.scheduledTime));

						var after = lstFlightArrival.Where(x => (TimeSpan.Parse(x.currentTime) > tspnCurrentTime))
												//.OrderByDescending(r => r.currentTime)
												.Take(Convert.ToInt32(flightRequest.count / 2)).ToList();
                        
						lstFlightArrivalFliterList.AddRange(after.OrderBy(x=>x.scheduledTime));*/


						lstFlightArrivalFliterList.AddRange(lstFlightArrival
							.Where(t => t.currentDateTime <= dtCurrentDate)
							.OrderByDescending(p => p.currentDateTime)
							.ThenByDescending(r => r.scheduledTime)
							.Take(Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(flightRequest.count / 2)))).ToList()
						);

						var getCounts = 0;
						getCounts = lstFlightArrivalFliterList != null && lstFlightArrivalFliterList.Count > 0
							? lstFlightArrivalFliterList.Count()
							: 0;
						if (getCounts < Math.Ceiling(Convert.ToDecimal(flightRequest.count / 2)))
							getCounts = flightRequest.count - getCounts;
						else
							getCounts = Convert.ToInt32(flightRequest.count / 2);

						lstFlightArrivalFliterList.AddRange(lstFlightArrival
							.Where(x => x.currentDateTime > dtCurrentDate)
							.OrderBy(r => r.currentDateTime)
							.ThenBy(r => r.scheduledTime)
							//.Take(Convert.ToInt32(flightRequest.count / 2)).ToList()
							.Take(getCounts).ToList()
						);

						lstFlightArrivalFliterList = lstFlightArrivalFliterList.OrderBy(o => o.scheduledTime).ToList();
					}
					else
					{
						lstFlightArrivalFliterList = lstFlightArrival;
					}

					if (flightRequest != null)
					{
						HttpContext.Current.Trace.Warn("flightRequest ::: " + flightRequest.flight + "   :: " +
														flightRequest.airlineCode + " ::: " + flightRequest.airportCode + " ::: " + flightRequest.ToString());

						if (!string.IsNullOrEmpty(flightRequest.date))
							lstFlightArrivalFliterList = lstFlightArrivalFliterList
								.Where(f => f.currentDate == flightRequest.date).ToList();
						if (!string.IsNullOrEmpty(flightRequest.flight) &&
							!string.IsNullOrEmpty(flightRequest.airportCode) &&
							flightRequest.airportCode == flightRequest.flight)
						{
							lstFlightArrivalFliterList = lstFlightArrivalFliterList
								.Where(f =>
									CultureInfo.CurrentCulture.CompareInfo.IndexOf(f.flight.Replace(" ", ""),
										flightRequest.flight.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
									|| CultureInfo.CurrentCulture.CompareInfo.IndexOf(f.originCity.Replace(" ", ""),
										flightRequest.airportCode.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
									|| CultureInfo.CurrentCulture.CompareInfo.IndexOf(
										f.originAirportCode.Replace(" ", ""),
										flightRequest.airportCode.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
								)
								.ToList();
						}
						else
						{
							if (!string.IsNullOrEmpty(flightRequest.flight))
								lstFlightArrivalFliterList = lstFlightArrivalFliterList
									.Where(f =>
										CultureInfo.CurrentCulture.CompareInfo.IndexOf(f.flight.Replace(" ", ""),
											flightRequest.flight.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
									)
									.ToList();

							if (!string.IsNullOrEmpty(flightRequest.airportCode))
								lstFlightArrivalFliterList = lstFlightArrivalFliterList
									.Where(f => CultureInfo.CurrentCulture.CompareInfo.IndexOf(
													f.originCity.Replace(" ", ""),
													flightRequest.airportCode.Replace(" ", ""),
													CompareOptions.IgnoreCase) >= 0
												|| CultureInfo.CurrentCulture.CompareInfo.IndexOf(
													f.originAirportCode.Replace(" ", ""),
													flightRequest.airportCode.Replace(" ", ""),
													CompareOptions.IgnoreCase) >= 0
									)
									.ToList();
						}

						if (!string.IsNullOrEmpty(flightRequest.airlineCode))
							lstFlightArrivalFliterList = lstFlightArrivalFliterList
								.Where(f =>
									CultureInfo.CurrentCulture.CompareInfo.IndexOf(f.airlineCode.Replace(" ", ""),
										flightRequest.airlineCode.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
									|| CultureInfo.CurrentCulture.CompareInfo.IndexOf(f.airlineName.Replace(" ", ""),
										flightRequest.airlineCode.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
								)
								.ToList();

						if (flightRequest != null && flightRequest.count > 0)
							lstFlightArrivalFliterList = lstFlightArrivalFliterList.Take(flightRequest.count).ToList();
					}

					objResponse = new CommonResponse<List<FlightDetail>>
					{
						success = true, message = "Successfully.", data = lstFlightArrivalFliterList,
						count = lstFlightArrivalFliterList.Count
					};
				}
				catch (Exception ex)
				{
					HttpContext.Current.Trace.Warn("Get :: " + ex.Message);
					objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
					objResponse = new CommonResponse<List<FlightDetail>> {success = false, error = objError};
				}
			}
			else
			{
				Common.clsCommonFunction.SetModelError(modalState, ref objError);
				objResponse = new CommonResponse<List<FlightDetail>> {success = false, error = objError};
			}

			//REQUEST RESPONSE LOGS
			objclsAPILOG.InsertAPILOG(clsAPILOG.GenerateLogContent(flightRequest, "GetArrivalFligths"),
				clsAPILOG.GenerateLogContent(objResponse, "GetArrivalFligths"));
		}

		public void GetDepartureFligths(FlightRequest flightRequest, ref CommonResponse<List<FlightDetail>> objResponse,
			ModelStateDictionary modalState)
		{
			var objError = new List<clsMessage>();
			if (modalState.IsValid)
			{
				try
				{
					//objFlightDTO.getData();
					var strCurrentTime = "";
					DateTime dtCurrentDate;

					var newDisplayTimeSpan = new TimeSpan(0, 20, 0);

					objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);
					var tspnCurrentTime = TimeSpan.Parse(strCurrentTime);

					var newDisplaySubsractTimeSpan = new TimeSpan(6, 0, 0);
					var newDisplayAdditionTimeSpan = new TimeSpan(12, 0, 0);

					var lstFlightDeparture = BindDepartureFlights();
					var lstFlightDepartureFliterList = new List<FlightDetail>();

					if (flightRequest.ReqType == RequestFromType.Dashboard && flightRequest.dateType == "today")
					{
						// SELECT DATA FROM -6 AND +12
						/*lstFlightDeparture = lstFlightDeparture
												.Where(
												(x =>
													(TimeSpan.Parse(x.currentTime) > tspnCurrentTime.Subtract(newDisplaySubsractTimeSpan) && TimeSpan.Parse(x.currentTime) <= tspnCurrentTime)
													|| (TimeSpan.Parse(x.currentTime) >= tspnCurrentTime && TimeSpan.Parse(x.currentTime) <= tspnCurrentTime.Add(newDisplayAdditionTimeSpan))
													)
												)
											.ToList();*/


						//ADD ALL FLIGHTS WHERE HAVE NOT ANY REMARKS
						/*lstFlightDepartureFliterList.AddRange(lstFlightDeparture.Where(a => a.remarks == "" || a.remarks == null).ToList());*/

						//ADD ALL DELAYED FLIGHTS
						/*lstFlightDepartureFliterList.AddRange(lstFlightDeparture.Where(a => a.remarks == FlightRemarks.Delayed).ToList());*/

						//ADD ALL FLIGHTS  EXECPT LANDED AND DELAYED
						//lstFlightDepartureFliterList.AddRange(lstFlightDeparture.Where(a => a.remarks != FlightRemarks.Delayed || a.remarks != FlightRemarks.Landed).ToList());

						//ADD OTHER FLIGHTS 20 MIN
						/*lstFlightDepartureFliterList.AddRange(lstFlightDeparture
													.Where(a => (a.remarks != FlightRemarks.Delayed)
														&& (TimeSpan.Parse(a.currentTime) > tspnCurrentTime.Subtract(newDisplayTimeSpan) || TimeSpan.Parse(a.currentTime) < tspnCurrentTime.Add(newDisplayTimeSpan))
														)
														.ToList()
													);*/

						lstFlightDeparture = (from item in lstFlightDeparture
								select
									new FlightDetail
									{
										//currentDateTime = Convert.ToDateTime(item.currentDate + " " + item.currentTime),
										currentDateTime = Convert.ToDateTime(
											objCommonFunction.ConvertDateTimeFormat(item.currentDate, item.currentTime,
												"")),
										currentDate = item.currentDate,
										currentTime = item.currentTime,
										airlineCode = item.airlineCode,
										scheduledTime = item.scheduledTime,
										scheduledDate = item.scheduledDate,
										terminal = item.terminal,
										originCity = item.originCity,
										airlineName = item.airlineName,
										airportCode = item.airportCode,
										delayed = item.delayed,
										destinationAirportCode = item.destinationAirportCode,
										destinationCity = item.destinationCity,
										flight = item.flight,
										flightNumber = item.flightNumber,
										originAirportCode = item.originAirportCode,
										remarks = item.remarks,
										remarksWithTime = item.remarksWithTime,
                                        checkinCounter = item.checkinCounter,
                                        gate = item.gate,
                                        actualTime = item.actualTime,
                                        actualDate = item.actualDate
									})
							.ToList();

						/*lstFlightDepartureFliterList.AddRange(lstFlightDeparture.Where(x => (TimeSpan.Parse(x.currentTime) > tspnCurrentTime))
												//.OrderByDescending(r => r.currentTime)
												.Take(Convert.ToInt32(flightRequest.count / 2)).ToList()
												);

						lstFlightDepartureFliterList.AddRange(lstFlightDeparture.Where(t => TimeSpan.Parse(t.currentTime) <= tspnCurrentTime)
												.OrderByDescending(p => p.currentTime)
												.Take(Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(flightRequest.count / 2)))).ToList()
												);*/

						lstFlightDepartureFliterList.AddRange(lstFlightDeparture
							.Where(t => t.currentDateTime <= dtCurrentDate)
							.OrderByDescending(p => p.currentDateTime)
							.ThenByDescending(r => r.scheduledTime)
							.Take(Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(flightRequest.count / 2)))).ToList()
						);

						var getCounts = 0;
						getCounts = lstFlightDepartureFliterList != null && lstFlightDepartureFliterList.Count > 0
							? lstFlightDepartureFliterList.Count()
							: 0;
						if (getCounts < Math.Ceiling(Convert.ToDecimal(flightRequest.count / 2)))
							getCounts = flightRequest.count - getCounts;
						else
							getCounts = Convert.ToInt32(flightRequest.count / 2);

						lstFlightDepartureFliterList.AddRange(lstFlightDeparture
							.Where(x => x.currentDateTime > dtCurrentDate)
							.OrderBy(r => r.currentDateTime)
							.ThenBy(r => r.scheduledTime)
							//.Take(Convert.ToInt32(flightRequest.count / 2))
							.Take(getCounts)
							.ToList()
						);

						lstFlightDepartureFliterList =
							lstFlightDepartureFliterList.OrderBy(o => o.scheduledTime).ToList();
					}
					else
					{
						lstFlightDepartureFliterList = lstFlightDeparture;
					}

					if (flightRequest != null)
					{
						if (!string.IsNullOrEmpty(flightRequest.date))
							lstFlightDepartureFliterList = lstFlightDepartureFliterList
								.Where(f => f.currentDate == flightRequest.date).ToList();
						//.Where(f => objCommonFunction.FormatStringToDateTime(f.currentDate, "DD/MM/YYYY") == flightRequest.date).ToList();
						HttpContext.Current.Trace.Warn("flightRequest ::: " + flightRequest.flight + "   :: " +
														flightRequest.airlineCode + " ::: " + flightRequest.airportCode + " ::: " + flightRequest.ToString());

						if (!string.IsNullOrEmpty(flightRequest.flight) &&
							!string.IsNullOrEmpty(flightRequest.airportCode) &&
							flightRequest.airportCode == flightRequest.flight)
						{
							lstFlightDepartureFliterList = lstFlightDepartureFliterList
								.Where(f =>
									CultureInfo.CurrentCulture.CompareInfo.IndexOf(f.flight.Replace(" ", ""),
										flightRequest.flight.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
									|| CultureInfo.CurrentCulture.CompareInfo.IndexOf(
										f.destinationCity.Replace(" ", ""), flightRequest.airportCode.Replace(" ", ""),
										CompareOptions.IgnoreCase) >= 0
									|| CultureInfo.CurrentCulture.CompareInfo.IndexOf(
										f.destinationAirportCode.Replace(" ", ""),
										flightRequest.airportCode.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
								)
								.ToList();
						}
						else
						{
							if (!string.IsNullOrEmpty(flightRequest.flight))
								lstFlightDepartureFliterList = lstFlightDepartureFliterList
									.Where(f =>
										CultureInfo.CurrentCulture.CompareInfo.IndexOf(f.flight.Replace(" ", ""),
											flightRequest.flight.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
									)
									.ToList();

							if (!string.IsNullOrEmpty(flightRequest.airportCode))
								lstFlightDepartureFliterList = lstFlightDepartureFliterList
									.Where(f =>
										CultureInfo.CurrentCulture.CompareInfo.IndexOf(
											f.destinationCity.Replace(" ", ""),
											flightRequest.airportCode.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
										|| CultureInfo.CurrentCulture.CompareInfo.IndexOf(
											f.destinationAirportCode.Replace(" ", ""),
											flightRequest.airportCode.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
									)
									.ToList();
						}

						if (!string.IsNullOrEmpty(flightRequest.airlineCode))
							lstFlightDepartureFliterList = lstFlightDepartureFliterList
								.Where(f =>
									CultureInfo.CurrentCulture.CompareInfo.IndexOf(f.airlineCode.Replace(" ", ""),
										flightRequest.airlineCode.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
									|| CultureInfo.CurrentCulture.CompareInfo.IndexOf(f.airlineName.Replace(" ", ""),
										flightRequest.airlineCode.Replace(" ", ""), CompareOptions.IgnoreCase) >= 0
								)
								.ToList();

						if (flightRequest != null && flightRequest.count > 0)
							lstFlightDepartureFliterList =
								lstFlightDepartureFliterList.Take(flightRequest.count).ToList();
					}

					objResponse = new CommonResponse<List<FlightDetail>>
					{
						success = true, message = "Successfully.", data = lstFlightDepartureFliterList,
						count = lstFlightDepartureFliterList.Count
					};
				}
				catch (Exception ex)
				{
					HttpContext.Current.Trace.Warn("Get :: " + ex.Message);
					objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
					objResponse = new CommonResponse<List<FlightDetail>> {success = false, error = objError};
				}
			}
			else
			{
				Common.clsCommonFunction.SetModelError(modalState, ref objError);
				objResponse = new CommonResponse<List<FlightDetail>> {success = false, error = objError};
			}

			//REQUEST RESPONSE LOGS
			objclsAPILOG.InsertAPILOG(clsAPILOG.GenerateLogContent(flightRequest, "GetDepartureFligths"),
				clsAPILOG.GenerateLogContent(objResponse, "GetDepartureFligths"));
		}

		public List<FlightDetail> CallArrivalFlightsAPI(ref string strFlightArrivalResponse)
		{
			var strCurrentTime = "";
			DateTime dtCurrentDate;
			var lstFlightArrival = new List<FlightDetail>();

			try
			{
				strFlightArrivalResponse = objCommonFunction.SendRequest(
					KeyValues.FlightArrivalServiceURL + KeyValues.FlightAPIKey, "", clsEnum.RequestMethod.Get, true);
				var objParseArrivalResult = JObject.Parse(Convert.ToString(strFlightArrivalResponse));
				lstFlightArrival =
					JsonConvert.DeserializeObject<List<FlightDetail>>(
						Convert.ToString(objParseArrivalResult["fidsData"]));

				dtCurrentDate = objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);

				objAPIRequestLOG.Data = "";
				objAPIRequestLOG.URL = KeyValues.FlightArrivalServiceURL + KeyValues.FlightAPIKey;
				objAPIRequestLOG.RequestedDate = dtCurrentDate;
				var strRequest = JsonConvert.SerializeObject(objAPIRequestLOG);
				objFlightDTO.InsertFlightThirdPartyRequestResponse(strRequest, strFlightArrivalResponse,
					clsEnum.FlightType.Arrival);
			}
			catch (Exception ex)
			{
				HttpContext.Current.Trace.Warn(" Exception :CallArrivalFlightsAPI :: : " + ex.Message);
			}

			return lstFlightArrival;
		}

		public List<FlightDetail> CallDepartFlightsAPI(ref string strFlightDepartureResponse)
		{
			var strCurrentTime = "";
			DateTime dtCurrentDate;
			var lstFlightDeparture = new List<FlightDetail>();

			try
			{
				strFlightDepartureResponse = objCommonFunction.SendRequest(
					KeyValues.FlightDepartureServiceURL + KeyValues.FlightAPIKey, "", clsEnum.RequestMethod.Get, true);
				var objParseArrivalResult = JObject.Parse(Convert.ToString(strFlightDepartureResponse));
				lstFlightDeparture =
					JsonConvert.DeserializeObject<List<FlightDetail>>(
						Convert.ToString(objParseArrivalResult["fidsData"]));

				dtCurrentDate = objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);

				objAPIRequestLOG.Data = "";
				objAPIRequestLOG.URL = KeyValues.FlightDepartureServiceURL + KeyValues.FlightAPIKey;
				objAPIRequestLOG.RequestedDate = dtCurrentDate;
				var strRequest = JsonConvert.SerializeObject(objAPIRequestLOG);
				objFlightDTO.InsertFlightThirdPartyRequestResponse(strRequest, strFlightDepartureResponse,
					clsEnum.FlightType.Departure);
			}
			catch (Exception ex)
			{
				HttpContext.Current.Trace.Warn(" Exception : CallDepartFlightsAPI :: : " + ex.Message);
			}

			return lstFlightDeparture;
		}

		public List<FlightDetail> BindArrivalFlights()
		{
			var strArrivalFlightCache = "";
			var lstFlightArrival = new List<FlightDetail>();

			try
			{
				strArrivalFlightCache = (string) Common.clsCommonFunction.GetCacheValue(CacheKey.ArrivalFlights);
			}
			catch (Exception ex)
			{
				strArrivalFlightCache = "";
			}

			if (!string.IsNullOrEmpty(strArrivalFlightCache))
			{
				var objParseArrivalResult = JObject.Parse(Convert.ToString(strArrivalFlightCache));
				lstFlightArrival =
					JsonConvert.DeserializeObject<List<FlightDetail>>(
						Convert.ToString(objParseArrivalResult["fidsData"]));
			}
			else
			{
				var objResponseDB =
					objFlightDTO.GetFlightResponse(clsEnum.FlightType.Arrival, KeyValues.FlightValidCacheAgeMinute);

				if (objResponseDB != null && objResponseDB.Count > 0)
				{
					var intValidTime = 0;
					foreach (var item in objResponseDB)
					{
						strArrivalFlightCache = Convert.ToString(item.data);
						intValidTime = Convert.ToInt32(item.mintime);
					}

					var objParseArrivalResult = JObject.Parse(Convert.ToString(strArrivalFlightCache));
					lstFlightArrival =
						JsonConvert.DeserializeObject<List<FlightDetail>>(
							Convert.ToString(objParseArrivalResult["fidsData"]));
					Common.clsCommonFunction.AddInCacheMemory(CacheKey.ArrivalFlights, strArrivalFlightCache,
						intValidTime);
				}
				else
				{
					lstFlightArrival = CallArrivalFlightsAPI(ref strArrivalFlightCache);
					Common.clsCommonFunction.AddInCacheMemory(CacheKey.ArrivalFlights, strArrivalFlightCache);
				}
			}

			return lstFlightArrival;
		}

		public List<FlightDetail> BindDepartureFlights()
		{
			var strDepartureFlightCache = "";
			var lstFlightDeparture = new List<FlightDetail>();

			try
			{
				strDepartureFlightCache = (string) Common.clsCommonFunction.GetCacheValue(CacheKey.DepartureFlights);
			}
			catch (Exception ex)
			{
				strDepartureFlightCache = "";
			}

			if (!string.IsNullOrEmpty(strDepartureFlightCache))
			{
				var objParseArrivalResult = JObject.Parse(Convert.ToString(strDepartureFlightCache));
				lstFlightDeparture =
					JsonConvert.DeserializeObject<List<FlightDetail>>(
						Convert.ToString(objParseArrivalResult["fidsData"]));
			}
			else
			{
				var objResponseDB =
					objFlightDTO.GetFlightResponse(clsEnum.FlightType.Departure, KeyValues.FlightValidCacheAgeMinute);

				if (objResponseDB != null && objResponseDB.Count > 0)
				{
					var intValidTime = 0;
					foreach (var item in objResponseDB)
					{
						strDepartureFlightCache = Convert.ToString(item.data);
						intValidTime = Convert.ToInt32(item.mintime);
					}

					var objParseArrivalResult = JObject.Parse(Convert.ToString(strDepartureFlightCache));
					lstFlightDeparture =
						JsonConvert.DeserializeObject<List<FlightDetail>>(
							Convert.ToString(objParseArrivalResult["fidsData"]));
					Common.clsCommonFunction.AddInCacheMemory(CacheKey.DepartureFlights, strDepartureFlightCache,
						intValidTime);
				}
				else
				{
					lstFlightDeparture = CallDepartFlightsAPI(ref strDepartureFlightCache);
					Common.clsCommonFunction.AddInCacheMemory(CacheKey.DepartureFlights, strDepartureFlightCache);
				}
			}

			return lstFlightDeparture;
		}

		public void GetFligthsLocations(ref CommonResponse<List<FlightRadar>> objResponse,
			ModelStateDictionary modalState)
		{
			var objError = new List<clsMessage>();
			if (modalState.IsValid)
			{
				try
				{
					var lstFlightLocations = new List<FlightRadar>();

					lstFlightLocations = CallAPIFlightRadar();

					objResponse = new CommonResponse<List<FlightRadar>>
					{
						success = true, message = "Successfully.", data = lstFlightLocations,
						count = lstFlightLocations.Count
					};
				}
				catch (Exception ex)
				{
					HttpContext.Current.Trace.Warn("Get :: " + ex.Message);
					objError.Add(new clsMessage {code = 500, message = "Internal Server Error."});
					objResponse = new CommonResponse<List<FlightRadar>> {success = false, error = objError};
				}
			}
			else
			{
				Common.clsCommonFunction.SetModelError(modalState, ref objError);
				objResponse = new CommonResponse<List<FlightRadar>> {success = false, error = objError};
			}

			//REQUEST RESPONSE LOGS
			objclsAPILOG.InsertAPILOG("", clsAPILOG.GenerateLogContent(objResponse, "GetFligthsLocations"));
		}

		public string amentFlightNumber(string flightNumber)
		{
			if (!string.IsNullOrEmpty(flightNumber))
				if (flightNumber.Substring(0, 2).Length < 3)
					flightNumber = flightNumber.Substring(0, 2) + flightNumber.Substring(2).PadLeft(3, '0');
			return flightNumber;
		}

		public List<FlightRadar> CallAPIFlightRadar()
		{
			var strCurrentTime = "";
			DateTime dtCurrentDate;
			var lstFlightLocation = new List<FlightRadar>();

			try
			{
				var strFlightLocationResponse = objCommonFunction.SendRequest(
					KeyValues.FlightRadarServiceURL + KeyValues.FlightAPIKey, "", clsEnum.RequestMethod.Get, true);
				var objParseArrivalResult = JObject.Parse(Convert.ToString(strFlightLocationResponse));
				lstFlightLocation =
					JsonConvert.DeserializeObject<List<FlightRadar>>(
						Convert.ToString(objParseArrivalResult["flightRadarData"]));

				var lstAFlightsTemp = BindArrivalFlights();
				var lstDFlightsTemp = BindDepartureFlights();

				var lstAFlights = new List<FlightDetail>();
				var lstDFlights = new List<FlightDetail>();

				foreach (var data in lstAFlightsTemp)
					if (!lstAFlights.Exists(x => x.flight == data.flight))
						lstAFlights.Add(data);

				foreach (var data in lstDFlightsTemp)
					if (!lstDFlights.Exists(x => x.flight == data.flight))
						lstDFlights.Add(data);

				var lstFlights = new List<FlightDetail>();
				lstFlights.AddRange(lstAFlights);
				lstFlights.AddRange(lstDFlights);

				var result = from fr in lstFlightLocation
					join f in lstFlights
						on amentFlightNumber(fr.flight) equals f.flight.Replace(" ", "")
					select new FlightRadar
					{
						latitude = fr.latitude,
						logo = f.airlineImageUrl,
						originCity = f.originCity,
						destinationCity = f.destinationCity,
						estimatedTime = f.estimatedTime == null ? f.scheduledTime : f.estimatedTime,
						estimatedDate = f.estimatedDate == null ? f.scheduledDate : f.estimatedDate,
						aircraftId = fr.aircraftId,
						destination = fr.destination,
						flight = fr.flight,
						longtitude = fr.longtitude,
						origin = fr.origin,
						track = fr.track,
						airlineName = f.airlineName,
						remarks = f.remarks,
						remarkClass = objFlightDetail.GetFlightStatus(f.remarks ?? "")
					};
				lstFlightLocation = result.ToList();


				//dtCurrentDate = objCommonFunction.GetCurrentBahrainTime(out strCurrentTime, out dtCurrentDate);

				//objAPIRequestLOG.Data = "";
				//objAPIRequestLOG.URL = KeyValues.FlightRadarServiceURL + KeyValues.FlightAPIKey;
				//objAPIRequestLOG.RequestedDate = dtCurrentDate;
				//string strRequest = JsonConvert.SerializeObject(objAPIRequestLOG);
				//objFlightDTO.InsertFlightThirdPartyRequestResponse(strRequest, strFlightLocationResponse, BusinessClassLibrary.LogicClass.Common.clsEnum.FlightType.Arrival);
			}
			catch (Exception ex)
			{
				HttpContext.Current.Trace.Warn(" Exception :CallAPIFlightRadar :: : " + ex.Message);
			}

			return lstFlightLocation;
		}
	}
}