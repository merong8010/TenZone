using UnityEngine;

public class MailList : CustomList<MailList.Data>
{
    public class Data
    {
        public string id;
        public string title;
        public string desc;
        public GoodsList.Data[] rewards;
        public long receiveDate;
    }
}
