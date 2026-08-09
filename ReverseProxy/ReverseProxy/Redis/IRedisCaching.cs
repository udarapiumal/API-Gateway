namespace ReverseProxy.Redis
{
    public interface IRedisCaching
    {
        T? GetData<T>(string key);
        void SetData<T>(string key, T data);
    }
}
