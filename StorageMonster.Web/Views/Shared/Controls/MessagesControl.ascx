﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.String>" %>
<%@ Import Namespace="StorageMonster.Web.Services.Extensions" %>

<%= Html.RequestSuccessInfo(new { @class = "alert alert-success" })%>
<%= Html.ValidationSummary(Model, new { @class = "alert alert-error" })%> 
<%= Html.RequestErrorInfo(new { @class = "alert alert-error" })%>  