# Set the base image as the .NET 6.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
COPY . ./
RUN dotnet publish ./GitHubActions.WebpageToPdf/GitHubActions.WebpageToPdf.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="Grzegorz Orwat"
LABEL repository="https://github.com/grzegorzorwat/GitHubActions.WebpageToPdf"
LABEL homepage="https://github.com/grzegorzorwat/GitHubActions.WebpageToPdfn"

# Label as GitHub action
LABEL com.github.actions.name="Create PDF for web page"
LABEL com.github.actions.description="A Github action that creates PDF for web page."
LABEL com.github.actions.icon="file-text"
LABEL com.github.actions.color="red"

# Relayer the .NET SDK, anew with the build output, install Google Chrome
FROM mcr.microsoft.com/dotnet/sdk:6.0

RUN apt-get update
RUN apt-get -y install gnupg
RUN wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - && \
    sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' && \
    apt-get update && \
    apt-get -y install xvfb gconf-service libasound2 libatk1.0-0 libc6 libcairo2 libcups2 \
    libdbus-1-3 libexpat1 libfontconfig1 libgcc1 libgconf-2-4 libgdk-pixbuf2.0-0 libglib2.0-0 \
    libgtk-3-0 libnspr4 libpango-1.0-0 libpangocairo-1.0-0 libstdc++6 libx11-6 libx11-xcb1 libxcb1 \
    libxcomposite1 libxcursor1 libxdamage1 libxext6 libxfixes3 libxi6 libxrandr2 libxrender1 libxss1 \
    libxtst6 ca-certificates fonts-liberation libnss3 lsb-release xdg-utils wget curl google-chrome-stable \
    fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf && \
    rm -rf /var/lib/apt/lists/*
RUN apt-get install -y fonts-noto-color-emoji

COPY --from=build-env /out .
ENTRYPOINT [ "dotnet", "/GitHubActions.WebpageToPdf.dll" ]