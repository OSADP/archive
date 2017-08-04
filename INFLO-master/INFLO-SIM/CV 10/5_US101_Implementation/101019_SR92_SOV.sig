<?xml version="1.0" encoding="UTF-8"?>
<sc id="3" name="SOV" frequency="1" steps="0" defaultIntergreenMatrix="0">
  <sgs>
    <sg id="1" name="Signal group" defaultSignalSequence="4">
      <defaultDurations>
        <defaultDuration display="1" duration="4000" />
        <defaultDuration display="3" duration="2000" />
      </defaultDurations>
    </sg>
  </sgs>
  <intergreenmatrices />
  <progs>
    <prog id="1" cycletime="6000" switchpoint="0" offset="2000" intergreens="0" fitness="0.000000" vehicleCount="0" name="Signal program">
      <sgs>
        <sg sg_id="1" signal_sequence="4">
          <cmds>
            <cmd display="3" begin="3000" />
            <cmd display="1" begin="5000" />
          </cmds>
          <fixedstates />
        </sg>
      </sgs>
    </prog>
  </progs>
  <stages />
  <interstageProgs />
  <stageProgs />
  <dailyProgLists />
</sc>