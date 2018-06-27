#from vapoursynth import core
import vapoursynth as vs
core = vs.get_core()
video = core.ffms2.Source(source="E:\\NaturalGrounding\\AOA\\Like a Cat (Dance Practice).mp4")
#video = core.std.Transpose(video)
#video = video.resize.Bicubic(format=vs.RGB24, matrix_in_s="709")
video.set_output()