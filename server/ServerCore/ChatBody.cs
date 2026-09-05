using System;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// [แก้เอง 2 ก.ย. 2026] ตัวช่วยอ่าน/เขียน "เนื้อข้อความ" ของแชท
///
/// 🐛 บั๊กที่แก้: ทั้ง <c>ServerPlayer.AcceptChat</c> และ <c>RadiotowerServer</c> เดิมเขียนว่า
/// <c>string body = message.Body as string;</c> แต่ในโปรโตคอลจริง <c>Message_.Body</c>
/// **ไม่เคยเป็น string** — ตัวเกมส่ง <c>RadioTalk { Text = ... }</c> มาเสมอ
/// (ดู <c>Message_.Pack</c> ที่ pack ตาม type code ของ Radio* เท่านั้น และ SocialSystem.cs:648)
///
/// ⇒ ผลคือ <c>body</c> เป็น null ทุกครั้ง โค้ดเลยกระโดดเข้าสาขา "ไม่ใช่ข้อความล้วน"
/// **เพดานความยาว 200 ตัวอักษรกับการกรองข้อความว่างจึงไม่เคยทำงานเลย**
/// ส่งข้อความยาวเท่าไรก็ได้ broadcast ออกไปหาทุกคนในโลก
/// </summary>
public static class ChatBody
{
    /// <summary>อ่านข้อความออกมาจาก Body — คืน null ถ้า Body ไม่ใช่ชนิดที่มีข้อความ</summary>
    public static string ReadText(object body)
    {
        switch (body)
        {
            case null: return null;
            case string s: return s;                       // เผื่อ client/mod ที่ส่งมาแบบง่าย
            case RadioTalk talk: return talk.Text;
            case RadioText text: return text.Text;
            case RadioNotice notice: return notice.Text;
            case RadioDictation dictation: return dictation.Text;
            default: return null;
        }
    }

    /// <summary>เขียนข้อความกลับลง Body โดยคงชนิดเดิมไว้ (ใช้ตอนตัดข้อความที่ยาวเกิน)</summary>
    public static object WriteText(object body, string text)
    {
        switch (body)
        {
            case string: return text;
            case RadioTalk talk: talk.Text = text; return talk;
            case RadioText t: t.Text = text; return t;
            case RadioNotice notice: notice.Text = text; return notice;
            case RadioDictation dictation: dictation.Text = text; return dictation;
            default: return body;
        }
    }
}
