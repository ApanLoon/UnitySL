using System;

namespace Assets.Scripts.Common
{
    public static class IndraConstants
    {
        /// <summary>
        /// "agent id" for things that should be done to ALL agents 
        /// </summary>
        public static readonly Guid LL_UUID_ALL_AGENTS = Guid.Parse("44e87126-e794-4ded-05b3-7c42da3d5cdb");

        /// <summary>
        /// Governor Linden's agent id.
        /// </summary>
        public static readonly Guid ALEXANDRIA_LINDEN_ID = Guid.Parse("ba2a564a-f0f1-4b82-9c61-b7520bfcd09f");
        public static readonly Guid GOVERNOR_LINDEN_ID = Guid.Parse("3d6181b0-6a4b-97ef-18d8-722652995cf1");
        
        /// <summary>
        /// Maintenance's group id.
        /// </summary>
        public static readonly Guid MAINTENANCE_GROUP_ID = Guid.Parse("dc7b21cd-3c89-fcaa-31c8-25f9ffd224cd");

        /// <summary>
        /// Grass Images
        /// </summary>
        public static readonly Guid IMG_SMOKE           = Guid.Parse("b4ba225c-373f-446d-9f7e-6cb7b5cf9b3d");  // VIEWER

        public static readonly Guid IMG_DEFAULT         = Guid.Parse("d2114404-dd59-4a4d-8e6c-49359e91bbf0");  // VIEWER

        public static readonly Guid IMG_SUN             = Guid.Parse("cce0f112-878f-4586-a2e2-a8f104bba271"); // dataserver
        public static readonly Guid IMG_MOON            = Guid.Parse("d07f6eed-b96a-47cd-b51d-400ad4a1c428"); // dataserver
        public static readonly Guid IMG_SHOT            = Guid.Parse("35f217a3-f618-49cf-bbca-c86d486551a9"); // dataserver
        public static readonly Guid IMG_SPARK           = Guid.Parse("d2e75ac1-d0fb-4532-820e-a20034ac814d"); // dataserver
        public static readonly Guid IMG_FIRE            = Guid.Parse("aca40aa8-44cf-44ca-a0fa-93e1a2986f82"); // dataserver
        public static readonly Guid IMG_FACE_SELECT     = Guid.Parse("a85ac674-cb75-4af6-9499-df7c5aaf7a28"); // face selector
        public static readonly Guid IMG_DEFAULT_AVATAR  = Guid.Parse("c228d1cf-4b5d-4ba8-84f4-899a0796aa97"); // dataserver
        public static readonly Guid IMG_INVISIBLE       = Guid.Parse("3a367d1c-bef1-6d43-7595-e88c1e3aadb3"); // dataserver

        public static readonly Guid IMG_EXPLOSION               = Guid.Parse("68edcf47-ccd7-45b8-9f90-1649d7f12806"); // On dataserver
        public static readonly Guid IMG_EXPLOSION_2             = Guid.Parse("21ce046c-83fe-430a-b629-c7660ac78d7c"); // On dataserver
        public static readonly Guid IMG_EXPLOSION_3             = Guid.Parse("fedea30a-1be8-47a6-bc06-337a04a39c4b"); // On dataserver
        public static readonly Guid IMG_EXPLOSION_4             = Guid.Parse("abf0d56b-82e5-47a2-a8ad-74741bb2c29e"); // On dataserver
        public static readonly Guid IMG_SMOKE_POOF              = Guid.Parse("1e63e323-5fe0-452e-92f8-b98bd0f764e3"); // On dataserver

        public static readonly Guid IMG_BIG_EXPLOSION_1         = Guid.Parse("5e47a0dc-97bf-44e0-8b40-de06718cee9d"); // On dataserver
        public static readonly Guid IMG_BIG_EXPLOSION_2         = Guid.Parse("9c8eca51-53d5-42a7-bb58-cef070395db8"); // On dataserver

        public static readonly Guid IMG_ALPHA_GRAD              = Guid.Parse("e97cf410-8e61-7005-ec06-629eba4cd1fb"); // VIEWER
        public static readonly Guid IMG_ALPHA_GRAD_2D           = Guid.Parse("38b86f85-2575-52a9-a531-23108d8da837"); // VIEWER
        public static readonly Guid IMG_TRANSPARENT             = Guid.Parse("8dcd4a48-2d37-4909-9f78-f7a9eb4ef903"); // VIEWER

        public static readonly Guid TERRAIN_DIRT_DETAIL         = Guid.Parse("0bc58228-74a0-7e83-89bc-5c23464bcec5"); // VIEWER
        public static readonly Guid TERRAIN_GRASS_DETAIL        = Guid.Parse("63338ede-0037-c4fd-855b-015d77112fc8"); // VIEWER
        public static readonly Guid TERRAIN_MOUNTAIN_DETAIL     = Guid.Parse("303cd381-8560-7579-23f1-f0a880799740"); // VIEWER
        public static readonly Guid TERRAIN_ROCK_DETAIL         = Guid.Parse("53a2f406-4895-1d13-d541-d2e3b86bc19c"); // VIEWER

        public static readonly Guid DEFAULT_WATER_NORMAL        = Guid.Parse("822ded49-9a6c-f61c-cb89-6df54f42cdf4"); // VIEWER

        public static readonly Guid IMG_USE_BAKED_HEAD   = Guid.Parse("5a9f4a74-30f2-821c-b88d-70499d3e7183");
        public static readonly Guid IMG_USE_BAKED_UPPER  = Guid.Parse("ae2de45c-d252-50b8-5c6e-19f39ce79317");
        public static readonly Guid IMG_USE_BAKED_LOWER  = Guid.Parse("24daea5f-0539-cfcf-047f-fbc40b2786ba");
        public static readonly Guid IMG_USE_BAKED_EYES   = Guid.Parse("52cc6bb6-2ee5-e632-d3ad-50197b1dcb8a");
        public static readonly Guid IMG_USE_BAKED_SKIRT  = Guid.Parse("43529ce8-7faa-ad92-165a-bc4078371687");
        public static readonly Guid IMG_USE_BAKED_HAIR   = Guid.Parse("09aac1fb-6bce-0bee-7d44-caac6dbb6c63");
        public static readonly Guid IMG_USE_BAKED_LEFTARM   = Guid.Parse("ff62763f-d60a-9855-890b-0c96f8f8cd98");
        public static readonly Guid IMG_USE_BAKED_LEFTLEG   = Guid.Parse("8e915e25-31d1-cc95-ae08-d58a47488251");
        public static readonly Guid IMG_USE_BAKED_AUX1   = Guid.Parse("9742065b-19b5-297c-858a-29711d539043");
        public static readonly Guid IMG_USE_BAKED_AUX2   = Guid.Parse("03642e83-2bd1-4eb9-34b4-4c47ed586d2d");
        public static readonly Guid IMG_USE_BAKED_AUX3   = Guid.Parse("edd51b77-fc10-ce7a-4b3d-011dfc349e4f");
    }
}
