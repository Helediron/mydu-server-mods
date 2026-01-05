# High resolution textures

Starting with client 1.4.12 honeycombs texture can have 2048 and 4096 pixel resolutions.


To enable, place your textures in texture packs "hcombshr_v1" and "hcombsxhr_v1" respectively, and set materialType to non_blending_l and non_blending_xl respectively.

## Example nqdef

```
{
  "worlds": {
    "voxel": {
      "materialDefinitions": {
        "hcombsxhr_v1": {
          "materialType": "non_blending_xl",
          "layers": [
            "c",
            "n",
            "mrao"
          ],
          "materials": {
      		"SuperTexture": {
              "texture": "resources_generated/supertexture_basename",
               "albedo": [
                0.5,
                0.5,
                0.5
              ]
              }
	        }
	    },
        "hcombsxhr_v1": {
          "materialType": "non_blending_xl",
          "layers": [
            "c",
            "n",
            "mrao"
          ],
          "materials": {
            "GigaTexture": {
               "texture": "resources_generated/gigatexture_basename",
              "albedo": [
                0.5,
                0.5,
                0.5
              ]
            }
          }
        }
      }
    }
  }
}

```