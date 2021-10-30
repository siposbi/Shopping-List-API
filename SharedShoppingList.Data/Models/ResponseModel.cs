namespace SharedShoppingList.Data.Models
{
    public class ResponseModel<T>
    {
        public ResponseModel()
        {
            IsSuccess = true;
            Message = "";
        }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ResponseModel<T> Exception()
        {
            IsSuccess = false;
            Message = "Something went wrong!";
            return this;
        }

        public ResponseModel<T> Unsuccessful(string msg)
        {
            IsSuccess = false;
            Message = msg;
            return this;
        }
    }
}