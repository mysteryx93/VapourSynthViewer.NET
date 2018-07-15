# This is appended to scripts to convert to COMPATBGR32 format for display
import vapoursynth as viewernet_vs
viewernet_node = viewernet_vs.get_output(index=0)
viewernet_format = viewernet_node.format
viewernet_params = { "format": viewernet_vs.COMPATBGR32 }
if viewernet_node.height <= 480:
    viewernet_matrix = "603"
else:
    viewernet_matrix = "709"
if viewernet_format is None or (viewernet_format.color_family != viewernet_vs.RGB and viewernet_format.id != viewernet_vs.COMPATBGR32):
    viewernet_params["matrix_in_s"] = viewernet_matrix
viewernet_node = viewernet_node.resize.Bicubic(**viewernet_params).std.FlipVertical()
viewernet_node.set_output()