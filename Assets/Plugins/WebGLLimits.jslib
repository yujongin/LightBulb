mergeInto(LibraryManager.library, {
    
    GetMaxVertexUniformVectors: function() {
        try {
            // WebGL 컨텍스트 생성
            var canvas = document.createElement('canvas');
            var gl = canvas.getContext('webgl2') || canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
            
            if (!gl) {
                console.warn('WebGL 컨텍스트를 생성할 수 없습니다.');
                return 254; // 기본값
            }
            
            // GL_MAX_VERTEX_UNIFORM_VECTORS 쿼리
            var maxVertexUniformVectors = gl.getParameter(gl.MAX_VERTEX_UNIFORM_VECTORS);
            
            // 정리
            canvas = null;
            gl = null;
            
            return maxVertexUniformVectors || 254;
        } catch (e) {
            console.error('GetMaxVertexUniformVectors 에러:', e);
            return 254;
        }
    },
    
    GetMaxFragmentUniformVectors: function() {
        try {
            var canvas = document.createElement('canvas');
            var gl = canvas.getContext('webgl2') || canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
            
            if (!gl) {
                return 221; // 기본값
            }
            
            var maxFragmentUniformVectors = gl.getParameter(gl.MAX_FRAGMENT_UNIFORM_VECTORS);
            
            canvas = null;
            gl = null;
            
            return maxFragmentUniformVectors || 221;
        } catch (e) {
            console.error('GetMaxFragmentUniformVectors 에러:', e);
            return 221;
        }
    },
    
    GetMaxVaryingVectors: function() {
        try {
            var canvas = document.createElement('canvas');
            var gl = canvas.getContext('webgl2') || canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
            
            if (!gl) {
                return 8; // 기본값
            }
            
            var maxVaryingVectors = gl.getParameter(gl.MAX_VARYING_VECTORS);
            
            canvas = null;
            gl = null;
            
            return maxVaryingVectors || 8;
        } catch (e) {
            console.error('GetMaxVaryingVectors 에러:', e);
            return 8;
        }
    },
    
    GetMaxVertexAttribs: function() {
        try {
            var canvas = document.createElement('canvas');
            var gl = canvas.getContext('webgl2') || canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
            
            if (!gl) {
                return 8; // 기본값
            }
            
            var maxVertexAttribs = gl.getParameter(gl.MAX_VERTEX_ATTRIBS);
            
            canvas = null;
            gl = null;
            
            return maxVertexAttribs || 8;
        } catch (e) {
            console.error('GetMaxVertexAttribs 에러:', e);
            return 8;
        }
    }
    
}); 