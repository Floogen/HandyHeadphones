using System;
using System.Collections.Generic;

namespace HandyHeadphones.API.Interfaces
{
    public interface IJsonAssetApi
    {
        void LoadAssets(string path);

        int GetHatId(string name);
    }
}
