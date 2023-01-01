{
  inputs.nixpkgs.url = "github:nixos/nixpkgs/nixos-22.11";
  outputs = { self, nixpkgs }:
    let pkgs = nixpkgs.legacyPackages.x86_64-linux;
    in {
      packages.x86_64-linux = {
        default = pkgs.buildDotnetModule rec {
          pname = "discord-client-proxy-v${version}";
          version = "1";
          dotnet-sdk = pkgs.dotnet-sdk_7;
          dotnet-runtime = pkgs.dotnet-aspnetcore_7;
          src = ./.;
          projectFile = [ "DiscordClientProxy/DiscordClientProxy.csproj" ];
          #nugetDeps = ./deps.nix; #generate with 
          nativeBuildInputs = with pkgs; [ pkg-config ];
        };
      };
      modules = {
        users = {
          users.users.discordclientproxy = {
            isSystemUser = true;
            home = "/var/lib/discordclientproxy";
            createHome = true;
            group = "discordclientproxy";
            extraGroups = [ ];
          };
          users.groups.botcore = { };
        };
        bots = {
          systemd.services = {
            "discordclientproxy" = {
              serviceConfig = {
                ExecStart =
                  "${self.packages.x86_64-linux.bots}/bin/DiscordClientProxy";
                Restart = "always";
                RestartSec = "5";
                User = "discordclientproxy";
              };
            };
          };
        };
      };
    };
}