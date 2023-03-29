FROM mcr.microsoft.com/cbl-mariner/base/nodejs:16

#RUN groupadd --gid 1000 node \
#  && useradd --uid 1000 --gid node --shell /bin/bash --create-home node

RUN ARCH= && dpkgArch="$(dpkg --print-architecture)" \
  && case "${dpkgArch##*-}" in \
    amd64) ARCH='x64';; \
    ppc64el) ARCH='ppc64le';; \
    s390x) ARCH='s390x';; \
    arm64) ARCH='arm64';; \
    armhf) ARCH='armv7l';; \
    i386) ARCH='x86';; \
    *) echo "unsupported architecture"; exit 1 ;; \
  esac

ARG NODE16_VERSION
ARG DEBIAN_FLAVOR
ENV NODE_VERSION ${NODE16_VERSION}
ENV NPM_CONFIG_LOGLEVEL info

ARG IMAGES_DIR=/tmp/oryx/images
RUN tdnf install nodejs \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs
RUN curl -L https://npmjs.org/install.sh | npm_install=6.14.15 sh
RUN ${IMAGES_DIR}/runtime/node/installDependencies.sh
RUN rm -rf /tmp/oryx

CMD [ "node" ]