var RecordEnum = {
	0x0003: { n:"BIFF2NUM", f:parse_BIFF2NUM },
	0x0004: { n:"BIFF2STR", f:parse_BIFF2STR },
	0x0006: { n:"Formula", f:parse_Formula },
	0x0009: { n:'BOF', f:parse_BOF },
	0x000a: { n:'EOF', f:parse_EOF },
	0x000c: { n:"CalcCount", f:parse_CalcCount },
	0x000d: { n:"CalcMode", f:parse_CalcMode },
	0x000e: { n:"CalcPrecision", f:parse_CalcPrecision },
	0x000f: { n:"CalcRefMode", f:parse_CalcRefMode },
	0x0010: { n:"CalcDelta", f:parse_CalcDelta },
	0x0011: { n:"CalcIter", f:parse_CalcIter },
	0x0012: { n:"Protect", f:parse_Protect },
	0x0013: { n:"Password", f:parse_Password },
	0x0014: { n:"Header", f:parse_Header },
	0x0015: { n:"Footer", f:parse_Footer },
	0x0017: { n:"ExternSheet", f:parse_ExternSheet },
	0x0018: { n:"Lbl", f:parse_Lbl },
	0x0019: { n:"WinProtect", f:parse_WinProtect },
	0x001a: { n:"VerticalPageBreaks", f:parse_VerticalPageBreaks },
	0x001b: { n:"HorizontalPageBreaks", f:parse_HorizontalPageBreaks },
	0x001c: { n:"Note", f:parse_Note },
	0x001d: { n:"Selection", f:parse_Selection },
	0x0022: { n:"Date1904", f:parse_Date1904 },
	0x0023: { n:"ExternName", f:parse_ExternName },
	0x0026: { n:"LeftMargin", f:parse_LeftMargin },
	0x0027: { n:"RightMargin", f:parse_RightMargin },
	0x0028: { n:"TopMargin", f:parse_TopMargin },
	0x0029: { n:"BottomMargin", f:parse_BottomMargin },
	0x002a: { n:"PrintRowCol", f:parse_PrintRowCol },
	0x002b: { n:"PrintGrid", f:parse_PrintGrid },
	0x002f: { n:"FilePass", f:parse_FilePass },
	0x0031: { n:"Font", f:parse_Font },
	0x0033: { n:"PrintSize", f:parse_PrintSize },
	0x003c: { n:"Continue", f:parse_Continue },
	0x003d: { n:"Window1", f:parse_Window1 },
	0x0040: { n:"Backup", f:parse_Backup },
	0x0041: { n:"Pane", f:parse_Pane },
	0x0042: { n:'CodePage', f:parse_CodePage },
	0x004d: { n:"Pls", f:parse_Pls },
	0x0050: { n:"DCon", f:parse_DCon },
	0x0051: { n:"DConRef", f:parse_DConRef },
	0x0052: { n:"DConName", f:parse_DConName },
	0x0055: { n:"DefColWidth", f:parse_DefColWidth },
	0x0059: { n:"XCT", f:parse_XCT },
	0x005a: { n:"CRN", f:parse_CRN },
	0x005b: { n:"FileSharing", f:parse_FileSharing },
	0x005c: { n:'WriteAccess', f:parse_WriteAccess },
	0x005d: { n:"Obj", f:parse_Obj },
	0x005e: { n:"Uncalced", f:parse_Uncalced },
	0x005f: { n:"CalcSaveRecalc", f:parse_CalcSaveRecalc },
	0x0060: { n:"Template", f:parse_Template },
	0x0061: { n:"Intl", f:parse_Intl },
	0x0063: { n:"ObjProtect", f:parse_ObjProtect },
	0x007d: { n:"ColInfo", f:parse_ColInfo },
	0x0080: { n:"Guts", f:parse_Guts },
	0x0081: { n:"WsBool", f:parse_WsBool },
	0x0082: { n:"GridSet", f:parse_GridSet },
	0x0083: { n:"HCenter", f:parse_HCenter },
	0x0084: { n:"VCenter", f:parse_VCenter },
	0x0085: { n:'BoundSheet8', f:parse_BoundSheet8 },
	0x0086: { n:"WriteProtect", f:parse_WriteProtect },
	0x008c: { n:"Country", f:parse_Country },
	0x008d: { n:"HideObj", f:parse_HideObj },
	0x0090: { n:"Sort", f:parse_Sort },
	0x0092: { n:"Palette", f:parse_Palette },
	0x0097: { n:"Sync", f:parse_Sync },
	0x0098: { n:"LPr", f:parse_LPr },
	0x0099: { n:"DxGCol", f:parse_DxGCol },
	0x009a: { n:"FnGroupName", f:parse_FnGroupName },
	0x009b: { n:"FilterMode", f:parse_FilterMode },
	0x009c: { n:"BuiltInFnGroupCount", f:parse_BuiltInFnGroupCount },
	0x009d: { n:"AutoFilterInfo", f:parse_AutoFilterInfo },
	0x009e: { n:"AutoFilter", f:parse_AutoFilter },
	0x00a0: { n:"Scl", f:parse_Scl },
	0x00a1: { n:"Setup", f:parse_Setup },
	0x00ae: { n:"ScenMan", f:parse_ScenMan },
	0x00af: { n:"SCENARIO", f:parse_SCENARIO },
	0x00b0: { n:"SxView", f:parse_SxView },
	0x00b1: { n:"Sxvd", f:parse_Sxvd },
	0x00b2: { n:"SXVI", f:parse_SXVI },
	0x00b4: { n:"SxIvd", f:parse_SxIvd },
	0x00b5: { n:"SXLI", f:parse_SXLI },
	0x00b6: { n:"SXPI", f:parse_SXPI },
	0x00b8: { n:"DocRoute", f:parse_DocRoute },
	0x00b9: { n:"RecipName", f:parse_RecipName },
	0x00bd: { n:"MulRk", f:parse_MulRk },
	0x00be: { n:"MulBlank", f:parse_MulBlank },
	0x00c1: { n:'Mms', f:parse_Mms },
	0x00c5: { n:"SXDI", f:parse_SXDI },
	0x00c6: { n:"SXDB", f:parse_SXDB },
	0x00c7: { n:"SXFDB", f:parse_SXFDB },
	0x00c8: { n:"SXDBB", f:parse_SXDBB },
	0x00c9: { n:"SXNum", f:parse_SXNum },
	0x00ca: { n:"SxBool", f:parse_SxBool },
	0x00cb: { n:"SxErr", f:parse_SxErr },
	0x00cc: { n:"SXInt", f:parse_SXInt },
	0x00cd: { n:"SXString", f:parse_SXString },
	0x00ce: { n:"SXDtr", f:parse_SXDtr },
	0x00cf: { n:"SxNil", f:parse_SxNil },
	0x00d0: { n:"SXTbl", f:parse_SXTbl },
	0x00d1: { n:"SXTBRGIITM", f:parse_SXTBRGIITM },
	0x00d2: { n:"SxTbpg", f:parse_SxTbpg },
	0x00d3: { n:"ObProj", f:parse_ObProj },
	0x00d5: { n:"SXStreamID", f:parse_SXStreamID },
	0x00d7: { n:"DBCell", f:parse_DBCell },
	0x00d8: { n:"SXRng", f:parse_SXRng },
	0x00d9: { n:"SxIsxoper", f:parse_SxIsxoper },
	0x00da: { n:"BookBool", f:parse_BookBool },
	0x00dc: { n:"DbOrParamQry", f:parse_DbOrParamQry },
	0x00dd: { n:"ScenarioProtect", f:parse_ScenarioProtect },
	0x00de: { n:"OleObjectSize", f:parse_OleObjectSize },
	0x00e0: { n:"XF", f:parse_XF },
	0x00e1: { n:'InterfaceHdr', f:parse_InterfaceHdr },
	0x00e2: { n:'InterfaceEnd', f:parse_InterfaceEnd },
	0x00e3: { n:"SXVS", f:parse_SXVS },
	0x00e5: { n:"MergeCells", f:parse_MergeCells },
	0x00e9: { n:"BkHim", f:parse_BkHim },
	0x00eb: { n:"MsoDrawingGroup", f:parse_MsoDrawingGroup },
	0x00ec: { n:"MsoDrawing", f:parse_MsoDrawing },
	0x00ed: { n:"MsoDrawingSelection", f:parse_MsoDrawingSelection },
	0x00ef: { n:"PhoneticInfo", f:parse_PhoneticInfo },
	0x00f0: { n:"SxRule", f:parse_SxRule },
	0x00f1: { n:"SXEx", f:parse_SXEx },
	0x00f2: { n:"SxFilt", f:parse_SxFilt },
	0x00f4: { n:"SxDXF", f:parse_SxDXF },
	0x00f5: { n:"SxItm", f:parse_SxItm },
	0x00f6: { n:"SxName", f:parse_SxName },
	0x00f7: { n:"SxSelect", f:parse_SxSelect },
	0x00f8: { n:"SXPair", f:parse_SXPair },
	0x00f9: { n:"SxFmla", f:parse_SxFmla },
	0x00fb: { n:"SxFormat", f:parse_SxFormat },
	0x00fc: { n:"SST", f:parse_SST },
	0x00fd: { n:"LabelSst", f:parse_LabelSst },
	0x00ff: { n:"ExtSST", f:parse_ExtSST },
	0x0100: { n:"SXVDEx", f:parse_SXVDEx },
	0x0103: { n:"SXFormula", f:parse_SXFormula },
	0x0122: { n:"SXDBEx", f:parse_SXDBEx },
	0x0137: { n:"RRDInsDel", f:parse_RRDInsDel },
	0x0138: { n:"RRDHead", f:parse_RRDHead },
	0x013b: { n:"RRDChgCell", f:parse_RRDChgCell },
	0x013d: { n:"RRTabId", f:parse_RRTabId },
	0x013e: { n:"RRDRenSheet", f:parse_RRDRenSheet },
	0x013f: { n:"RRSort", f:parse_RRSort },
	0x0140: { n:"RRDMove", f:parse_RRDMove },
	0x014a: { n:"RRFormat", f:parse_RRFormat },
	0x014b: { n:"RRAutoFmt", f:parse_RRAutoFmt },
	0x014d: { n:"RRInsertSh", f:parse_RRInsertSh },
	0x014e: { n:"RRDMoveBegin", f:parse_RRDMoveBegin },
	0x014f: { n:"RRDMoveEnd", f:parse_RRDMoveEnd },
	0x0150: { n:"RRDInsDelBegin", f:parse_RRDInsDelBegin },
	0x0151: { n:"RRDInsDelEnd", f:parse_RRDInsDelEnd },
	0x0152: { n:"RRDConflict", f:parse_RRDConflict },
	0x0153: { n:"RRDDefName", f:parse_RRDDefName },
	0x0154: { n:"RRDRstEtxp", f:parse_RRDRstEtxp },
	0x015f: { n:"LRng", f:parse_LRng },
	0x0160: { n:"UsesELFs", f:parse_UsesELFs },
	0x0161: { n:"DSF", f:parse_DSF },
	0x0191: { n:"CUsr", f:parse_CUsr },
	0x0192: { n:"CbUsr", f:parse_CbUsr },
	0x0193: { n:"UsrInfo", f:parse_UsrInfo },
	0x0194: { n:"UsrExcl", f:parse_UsrExcl },
	0x0195: { n:"FileLock", f:parse_FileLock },
	0x0196: { n:"RRDInfo", f:parse_RRDInfo },
	0x0197: { n:"BCUsrs", f:parse_BCUsrs },
	0x0198: { n:"UsrChk", f:parse_UsrChk },
	0x01a9: { n:"UserBView", f:parse_UserBView },
	0x01aa: { n:"UserSViewBegin", f:parse_UserSViewBegin },
	0x01ab: { n:"UserSViewEnd", f:parse_UserSViewEnd },
	0x01ac: { n:"RRDUserView", f:parse_RRDUserView },
	0x01ad: { n:"Qsi", f:parse_Qsi },
	0x01ae: { n:"SupBook", f:parse_SupBook },
	0x01af: { n:"Prot4Rev", f:parse_Prot4Rev },
	0x01b0: { n:"CondFmt", f:parse_CondFmt },
	0x01b1: { n:"CF", f:parse_CF },
	0x01b2: { n:"DVal", f:parse_DVal },
	0x01b5: { n:"DConBin", f:parse_DConBin },
	0x01b6: { n:"TxO", f:parse_TxO },
	0x01b7: { n:"RefreshAll", f:parse_RefreshAll },
	0x01b8: { n:"HLink", f:parse_HLink },
	0x01b9: { n:"Lel", f:parse_Lel },
	0x01ba: { n:"CodeName", f:parse_CodeName },
	0x01bb: { n:"SXFDBType", f:parse_SXFDBType },
	0x01bc: { n:"Prot4RevPass", f:parse_Prot4RevPass },
	0x01bd: { n:"ObNoMacros", f:parse_ObNoMacros },
	0x01be: { n:"Dv", f:parse_Dv },
	0x01c0: { n:"Excel9File", f:parse_Excel9File },
	0x01c1: { n:"RecalcId", f:parse_RecalcId, r:2},
	0x01c2: { n:"EntExU2", f:parse_EntExU2 },
	0x0200: { n:"Dimensions", f:parse_Dimensions },
	0x0201: { n:"Blank", f:parse_Blank },
	0x0203: { n:"Number", f:parse_Number },
	0x0204: { n:"Label", f:parse_Label },
	0x0205: { n:"BoolErr", f:parse_BoolErr },
	0x0207: { n:"String", f:parse_String },
	0x0208: { n:'Row', f:parse_Row },
	0x020b: { n:"Index", f:parse_Index },
	0x0221: { n:"Array", f:parse_Array },
	0x0225: { n:"DefaultRowHeight", f:parse_DefaultRowHeight },
	0x0236: { n:"Table", f:parse_Table },
	0x023e: { n:"Window2", f:parse_Window2 },
	0x027e: { n:"RK", f:parse_RK },
	0x0293: { n:"Style", f:parse_Style },
	0x0418: { n:"BigName", f:parse_BigName },
	0x041e: { n:"Format", f:parse_Format },
	0x043c: { n:"ContinueBigName", f:parse_ContinueBigName },
	0x04bc: { n:"ShrFmla", f:parse_ShrFmla },
	0x0800: { n:"HLinkTooltip", f:parse_HLinkTooltip },
	0x0801: { n:"WebPub", f:parse_WebPub },
	0x0802: { n:"QsiSXTag", f:parse_QsiSXTag },
	0x0803: { n:"DBQueryExt", f:parse_DBQueryExt },
	0x0804: { n:"ExtString", f:parse_ExtString },
	0x0805: { n:"TxtQry", f:parse_TxtQry },
	0x0806: { n:"Qsir", f:parse_Qsir },
	0x0807: { n:"Qsif", f:parse_Qsif },
	0x0808: { n:"RRDTQSIF", f:parse_RRDTQSIF },
	0x0809: { n:'BOF', f:parse_BOF },
	0x080a: { n:"OleDbConn", f:parse_OleDbConn },
	0x080b: { n:"WOpt", f:parse_WOpt },
	0x080c: { n:"SXViewEx", f:parse_SXViewEx },
	0x080d: { n:"SXTH", f:parse_SXTH },
	0x080e: { n:"SXPIEx", f:parse_SXPIEx },
	0x080f: { n:"SXVDTEx", f:parse_SXVDTEx },
	0x0810: { n:"SXViewEx9", f:parse_SXViewEx9 },
	0x0812: { n:"ContinueFrt", f:parse_ContinueFrt },
	0x0813: { n:"RealTimeData", f:parse_RealTimeData },
	0x0850: { n:"ChartFrtInfo", f:parse_ChartFrtInfo },
	0x0851: { n:"FrtWrapper", f:parse_FrtWrapper },
	0x0852: { n:"StartBlock", f:parse_StartBlock },
	0x0853: { n:"EndBlock", f:parse_EndBlock },
	0x0854: { n:"StartObject", f:parse_StartObject },
	0x0855: { n:"EndObject", f:parse_EndObject },
	0x0856: { n:"CatLab", f:parse_CatLab },
	0x0857: { n:"YMult", f:parse_YMult },
	0x0858: { n:"SXViewLink", f:parse_SXViewLink },
	0x0859: { n:"PivotChartBits", f:parse_PivotChartBits },
	0x085a: { n:"FrtFontList", f:parse_FrtFontList },
	0x0862: { n:"SheetExt", f:parse_SheetExt },
	0x0863: { n:"BookExt", f:parse_BookExt, r:12},
	0x0864: { n:"SXAddl", f:parse_SXAddl },
	0x0865: { n:"CrErr", f:parse_CrErr },
	0x0866: { n:"HFPicture", f:parse_HFPicture },
	0x0867: { n:'FeatHdr', f:parse_FeatHdr },
	0x0868: { n:"Feat", f:parse_Feat },
	0x086a: { n:"DataLabExt", f:parse_DataLabExt },
	0x086b: { n:"DataLabExtContents", f:parse_DataLabExtContents },
	0x086c: { n:"CellWatch", f:parse_CellWatch },
	0x0871: { n:"FeatHdr11", f:parse_FeatHdr11 },
	0x0872: { n:"Feature11", f:parse_Feature11 },
	0x0874: { n:"DropDownObjIds", f:parse_DropDownObjIds },
	0x0875: { n:"ContinueFrt11", f:parse_ContinueFrt11 },
	0x0876: { n:"DConn", f:parse_DConn },
	0x0877: { n:"List12", f:parse_List12 },
	0x0878: { n:"Feature12", f:parse_Feature12 },
	0x0879: { n:"CondFmt12", f:parse_CondFmt12 },
	0x087a: { n:"CF12", f:parse_CF12 },
	0x087b: { n:"CFEx", f:parse_CFEx },
	0x087c: { n:"XFCRC", f:parse_XFCRC, r:12 },
	0x087d: { n:"XFExt", f:parse_XFExt, r:12 },
	0x087e: { n:"AutoFilter12", f:parse_AutoFilter12 },
	0x087f: { n:"ContinueFrt12", f:parse_ContinueFrt12 },
	0x0884: { n:"MDTInfo", f:parse_MDTInfo },
	0x0885: { n:"MDXStr", f:parse_MDXStr },
	0x0886: { n:"MDXTuple", f:parse_MDXTuple },
	0x0887: { n:"MDXSet", f:parse_MDXSet },
	0x0888: { n:"MDXProp", f:parse_MDXProp },
	0x0889: { n:"MDXKPI", f:parse_MDXKPI },
	0x088a: { n:"MDB", f:parse_MDB },
	0x088b: { n:"PLV", f:parse_PLV },
	0x088c: { n:"Compat12", f:parse_Compat12, r:12 },
	0x088d: { n:"DXF", f:parse_DXF },
	0x088e: { n:"TableStyles", f:parse_TableStyles, r:12 },
	0x088f: { n:"TableStyle", f:parse_TableStyle },
	0x0890: { n:"TableStyleElement", f:parse_TableStyleElement },
	0x0892: { n:"StyleExt", f:parse_StyleExt },
	0x0893: { n:"NamePublish", f:parse_NamePublish },
	0x0894: { n:"NameCmt", f:parse_NameCmt },
	0x0895: { n:"SortData", f:parse_SortData },
	0x0896: { n:"Theme", f:parse_Theme, r:12 },
	0x0897: { n:"GUIDTypeLib", f:parse_GUIDTypeLib },
	0x0898: { n:"FnGrp12", f:parse_FnGrp12 },
	0x0899: { n:"NameFnGrp12", f:parse_NameFnGrp12 },
	0x089a: { n:"MTRSettings", f:parse_MTRSettings, r:12 },
	0x089b: { n:"CompressPictures", f:parse_CompressPictures },
	0x089c: { n:"HeaderFooter", f:parse_HeaderFooter },
	0x089d: { n:"CrtLayout12", f:parse_CrtLayout12 },
	0x089e: { n:"CrtMlFrt", f:parse_CrtMlFrt },
	0x089f: { n:"CrtMlFrtContinue", f:parse_CrtMlFrtContinue },
	0x08a3: { n:"ForceFullCalculation", f:parse_ForceFullCalculation },
	0x08a4: { n:"ShapePropsStream", f:parse_ShapePropsStream },
	0x08a5: { n:"TextPropsStream", f:parse_TextPropsStream },
	0x08a6: { n:"RichTextStream", f:parse_RichTextStream },
	0x08a7: { n:"CrtLayout12A", f:parse_CrtLayout12A },
	0x1001: { n:"Units", f:parse_Units },
	0x1002: { n:"Chart", f:parse_Chart },
	0x1003: { n:"Series", f:parse_Series },
	0x1006: { n:"DataFormat", f:parse_DataFormat },
	0x1007: { n:"LineFormat", f:parse_LineFormat },
	0x1009: { n:"MarkerFormat", f:parse_MarkerFormat },
	0x100a: { n:"AreaFormat", f:parse_AreaFormat },
	0x100b: { n:"PieFormat", f:parse_PieFormat },
	0x100c: { n:"AttachedLabel", f:parse_AttachedLabel },
	0x100d: { n:"SeriesText", f:parse_SeriesText },
	0x1014: { n:"ChartFormat", f:parse_ChartFormat },
	0x1015: { n:"Legend", f:parse_Legend },
	0x1016: { n:"SeriesList", f:parse_SeriesList },
	0x1017: { n:"Bar", f:parse_Bar },
	0x1018: { n:"Line", f:parse_Line },
	0x1019: { n:"Pie", f:parse_Pie },
	0x101a: { n:"Area", f:parse_Area },
	0x101b: { n:"Scatter", f:parse_Scatter },
	0x101c: { n:"CrtLine", f:parse_CrtLine },
	0x101d: { n:"Axis", f:parse_Axis },
	0x101e: { n:"Tick", f:parse_Tick },
	0x101f: { n:"ValueRange", f:parse_ValueRange },
	0x1020: { n:"CatSerRange", f:parse_CatSerRange },
	0x1021: { n:"AxisLine", f:parse_AxisLine },
	0x1022: { n:"CrtLink", f:parse_CrtLink },
	0x1024: { n:"DefaultText", f:parse_DefaultText },
	0x1025: { n:"Text", f:parse_Text },
	0x1026: { n:"FontX", f:parse_FontX },
	0x1027: { n:"ObjectLink", f:parse_ObjectLink },
	0x1032: { n:"Frame", f:parse_Frame },
	0x1033: { n:"Begin", f:parse_Begin },
	0x1034: { n:"End", f:parse_End },
	0x1035: { n:"PlotArea", f:parse_PlotArea },
	0x103a: { n:"Chart3d", f:parse_Chart3d },
	0x103c: { n:"PicF", f:parse_PicF },
	0x103d: { n:"DropBar", f:parse_DropBar },
	0x103e: { n:"Radar", f:parse_Radar },
	0x103f: { n:"Surf", f:parse_Surf },
	0x1040: { n:"RadarArea", f:parse_RadarArea },
	0x1041: { n:"AxisParent", f:parse_AxisParent },
	0x1043: { n:"LegendException", f:parse_LegendException },
	0x1044: { n:"ShtProps", f:parse_ShtProps },
	0x1045: { n:"SerToCrt", f:parse_SerToCrt },
	0x1046: { n:"AxesUsed", f:parse_AxesUsed },
	0x1048: { n:"SBaseRef", f:parse_SBaseRef },
	0x104a: { n:"SerParent", f:parse_SerParent },
	0x104b: { n:"SerAuxTrend", f:parse_SerAuxTrend },
	0x104e: { n:"IFmtRecord", f:parse_IFmtRecord },
	0x104f: { n:"Pos", f:parse_Pos },
	0x1050: { n:"AlRuns", f:parse_AlRuns },
	0x1051: { n:"BRAI", f:parse_BRAI },
	0x105b: { n:"SerAuxErrBar", f:parse_SerAuxErrBar },
	0x105c: { n:"ClrtClient", f:parse_ClrtClient },
	0x105d: { n:"SerFmt", f:parse_SerFmt },
	0x105f: { n:"Chart3DBarShape", f:parse_Chart3DBarShape },
	0x1060: { n:"Fbi", f:parse_Fbi },
	0x1061: { n:"BopPop", f:parse_BopPop },
	0x1062: { n:"AxcExt", f:parse_AxcExt },
	0x1063: { n:"Dat", f:parse_Dat },
	0x1064: { n:"PlotGrowth", f:parse_PlotGrowth },
	0x1065: { n:"SIIndex", f:parse_SIIndex },
	0x1066: { n:"GelFrame", f:parse_GelFrame },
	0x1067: { n:"BopPopCustom", f:parse_BopPopCustom },
	0x1068: { n:"Fbi2", f:parse_Fbi2 },

	/* These are specified in an older version of the spec */
	0x0016: { n:"ExternCount", f:parsenoop },
	0x007e: { n:"RK", f:parsenoop }, /* Not necessarily same as 0x027e */
	0x007f: { n:"ImData", f:parsenoop },
	0x0087: { n:"Addin", f:parsenoop },
	0x0088: { n:"Edg", f:parsenoop },
	0x0089: { n:"Pub", f:parsenoop },
	0x0091: { n:"Sub", f:parsenoop },
	0x0094: { n:"LHRecord", f:parsenoop },
	0x0095: { n:"LHNGraph", f:parsenoop },
	0x0096: { n:"Sound", f:parsenoop },
	0x00a9: { n:"CoordList", f:parsenoop },
	0x00ab: { n:"GCW", f:parsenoop },
	0x00bc: { n:"ShrFmla", f:parsenoop }, /* Not necessarily same as 0x04bc */
	0x00c2: { n:"AddMenu", f:parsenoop },
	0x00c3: { n:"DelMenu", f:parsenoop },
	0x00d6: { n:"RString", f:parsenoop },
	0x00df: { n:"UDDesc", f:parsenoop },
	0x00ea: { n:"TabIdConf", f:parsenoop },
	0x0162: { n:"XL5Modify", f:parsenoop },
	0x01a5: { n:"FileSharing2", f:parsenoop },
	0x0218: { n:"Name", f:parsenoop },
	0x0223: { n:"ExternName", f:parse_ExternName },
	0x0231: { n:"Font", f:parsenoop },
	0x0406: { n:"Formula", f:parse_Formula },
	0x086d: { n:"FeatInfo", f:parsenoop },
	0x0873: { n:"FeatInfo11", f:parsenoop },
	0x0881: { n:"SXAddl12", f:parsenoop },
	0x08c0: { n:"AutoWebPub", f:parsenoop },
	0x08c1: { n:"ListObj", f:parsenoop },
	0x08c2: { n:"ListField", f:parsenoop },
	0x08c3: { n:"ListDV", f:parsenoop },
	0x08c4: { n:"ListCondFmt", f:parsenoop },
	0x08c5: { n:"ListCF", f:parsenoop },
	0x08c6: { n:"FMQry", f:parsenoop },
	0x08c7: { n:"FMSQry", f:parsenoop },
	0x08c8: { n:"PLV", f:parsenoop }, /* supposedly PLV for Excel 11 */
	0x08c9: { n:"LnExt", f:parsenoop },
	0x08ca: { n:"MkrExt", f:parsenoop },
	0x08cb: { n:"CrtCoopt", f:parsenoop },

	0x0000: {}
};

