﻿namespace DealBot.Domain.Enums;

public enum States
{
    None,
    Restart,
    WaitingForSelectLanguage,
    WaitingForSendPhoneNumber,
    WaitingForSelectMenu,
    WaitingForSubscribeToChannel,
    WaitingForSelectCardOption,
    WaitingForSelectAddressOption,
    WaitingForSelectStoreContactOption,
    WaitingForSendComment,
    WaitingForSelectSettings,
    WaitingForSelectChangePersonalInfo,
    WaitingForFirstSelectLanguage,
    WaitingForSendFirstName,
    WaitingForSendLastName,
    WaitingForSendEmail,
    WaitingForSelectGender,
    CommentReceived,
    WaitingForSelectDateOfBirth,
    WaitingForSelectDateOfBirthYear1,
    WaitingForSelectDateOfBirthYear2,
    WaitingForSelectDateOfBirthYear3,
    WaitingForSelectDateOfBirthYear4,
    WaitingForSelectDateOfBirthYear5,
    WaitingForSelectDateOfBirthMonth,
    WaitingForSelectDateOfBirthDay,
    WaitingForSelectBotSettings,
    WaitingForSendName,
    WaitingForSendBotPic,
    WaitingForSendAbout,
    WaitingForSendUserId,
    WaitingForSelectUserMenu,
    WaitingForSendProductPrice,
    WaitingForSendSalesAmount,
    WaitingForSendMessage,
    WaitingForSelectTransaction,
    WaitingForSendMessageToDeveloper,
}