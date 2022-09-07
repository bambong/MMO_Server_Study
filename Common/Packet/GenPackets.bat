START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCopy /Y GenPackets.cs "../../DummyClient/Packet"
XCopy /Y GenPackets.cs "../../Server/Packet"
XCopy /Y ClientPacketManager.cs "../../DummyClient/Packet"
XCopy /Y ServerPacketManager.cs "../../Server/Packet"
